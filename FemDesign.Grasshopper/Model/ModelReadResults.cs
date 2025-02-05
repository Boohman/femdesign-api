﻿// https://strusoft.com/
using System;
using System.Linq;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using FemDesign.Calculate;

namespace FemDesign.Grasshopper
{
    public class ModelReadResults : GH_Component
    {
        public ModelReadResults() : base("Model.ReadResults", "ReadResults", "Read Results from .str file or model.", CategoryName.Name(), SubCategoryName.Cat6())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("StrPath", "StrPath", "File path to FEM-Design model (.str) file.", GH_ParamAccess.item);
            pManager.AddTextParameter("ResultTypes", "ResultTypes", "Results to be extracted from model. This might require the model to have been analysed. Item or list.", GH_ParamAccess.list);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddTextParameter("Case/Combination Name", "Case/Comb Name", "Name of Load Case/Load Combination for which to return the results. By default will return all case and combinations.", GH_ParamAccess.list);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("Options", "Options", "Settings for output location. Default is 'ByStep' and 'Vertices'", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("Units", "Units", "Specify the Result Units for some specific type. \n" +
                "Default Units are: Length.m, Angle.deg, SectionalData.m, Force.kN, Mass.kg, Displacement.m, Stress.Pa", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddBooleanParameter("RunNode", "RunNode", "If true node will execute. If false node will not execute.", GH_ParamAccess.item, false);
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Results", "Results.", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get input
            string filePath = null;

            DA.GetData("StrPath", ref filePath);
            if (filePath == null)
            {
                return;
            }

            List<string> resultTypes = new List<string>();
            DA.GetDataList("ResultTypes", resultTypes);

            bool hasFiniteElent = false;
            if (resultTypes.Contains("FiniteElement"))
            {
                resultTypes.Insert(0, "FemNode");
                resultTypes.Insert(1, "FemBar");
                resultTypes.Insert(2, "FemShell");
                resultTypes.Remove("FiniteElement");
                hasFiniteElent = true;
            }

            List<string> caseCombo = new List<string>();
            DA.GetDataList("Case/Combination Name", caseCombo);


            FemDesign.Calculate.Options options = FemDesign.Calculate.Options.Default();
            DA.GetData("Options", ref options);

            bool runNode = true;
            if (!DA.GetData("RunNode", ref runNode))
            {
                // pass
            }

            // Units
            var units = Results.UnitResults.Default();
            DA.GetData("Units", ref units);

            // RunNode
            if (runNode)
            {
                var notValidResultTypes = new List<string>();
                var _resultTypes = resultTypes.Select(r =>
                {
                    var sucess = Results.ResultTypes.All.TryGetValue(r, out Type value);
                    if (sucess)
                        return value;
                    else
                    {
                        notValidResultTypes.Add(r);
                        return null;
                    }
                });
                if (notValidResultTypes.Count() != 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The following strings are not valid result types: " + string.Join(", ", notValidResultTypes));
                    return;
                }

                // Create Bsc files from resultTypes
                List<MapCase> mapCase = new List<MapCase>();
                foreach(var caseComb in caseCombo)
                {
                    mapCase.Add(new MapCase(caseComb));
                }

                var allCase = mapCase.Count == 0 ? true: false;
                
                var bscPathsFromResultTypes = Calculate.Bsc.BscPathFromResultTypes(_resultTypes, filePath, units, options, allCase);

                var app = new FemDesign.Calculate.Application();
                // Create FdScript
                var fdScript = FemDesign.Calculate.FdScript.ReadLoadCase(filePath, bscPathsFromResultTypes, mapCase);

                // Run FdScript
                bool hasExited = app.RunFdScript(fdScript, false, true, false);

                // Read model and results
                var model = Model.DeserializeFromFilePath(fdScript.StruxmlPath);


                if (_FileName.IsASCII(filePath))
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File path has special characters. This might cause problems.");


                IEnumerable<Results.IResult> results = Enumerable.Empty<Results.IResult>();


                List<Results.FemNode> feaNodeRes = new List<Results.FemNode>();
                List<Results.FemBar> feaBarRes = new List<Results.FemBar>();
                List<Results.FemShell> feaShellRes = new List<Results.FemShell>();

                if (resultTypes != null && resultTypes.Any())
                {
                    foreach (var cmd in fdScript.CmdListGen)
                    {
                        string path = cmd.OutFile;
                        try
                        {
                            if (path.Contains("FemNode"))
                            {
                                feaNodeRes = Results.ResultsReader.Parse(path).Cast<Results.FemNode>().ToList();
                            }
                            else if (path.Contains("FemBar"))
                            {
                                feaBarRes = Results.ResultsReader.Parse(path).Cast<Results.FemBar>().ToList();
                            }
                            else if (path.Contains("FemShell"))
                            {
                                feaShellRes = Results.ResultsReader.Parse(path).Cast<Results.FemShell>().ToList();
                            }
                            else
                            {
                                var _results = Results.ResultsReader.Parse(path);
                                results = results.Concat(_results);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.InnerException.Message);
                        }
                    }
                }


                var resultGroups = results.GroupBy(t => t.GetType()).ToList();
                // Convert Data in DataTree structure
                var resultsTree = new DataTree<object>();

                var i = 0;
                foreach (var resGroup in resultGroups)
                {
                    resultsTree.AddRange(resGroup.AsEnumerable(), new GH_Path(i));
                    i++;
                }


                Results.FiniteElement FiniteElement = null;
                if (hasFiniteElent)
                {
                    FiniteElement = new FemDesign.Results.FiniteElement(feaNodeRes, feaBarRes, feaShellRes);
                    resultsTree.Add(FiniteElement, new GH_Path(i));
                }

                DA.SetDataTree(0, resultsTree);
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "RunNode is set to false!");
                return;
            }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.ModelReadStr;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{A098B67B-6AF4-40FA-981C-C2DE9225879E}"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

    }
}