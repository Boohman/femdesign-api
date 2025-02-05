﻿// https://strusoft.com/
using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using System.Text.RegularExpressions;

namespace FemDesign.Grasshopper
{
    public class ModelReadFromFile : GH_Component
    {
        public ModelReadFromFile() : base("Model.ReadFromFile", "ReadFromFile", "Read model from .struxml or .str. Note: Only supported elements will loaded from the .struxml model.", CategoryName.Name(), SubCategoryName.Cat6())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FilePath", "FilePath", "File path to .struxml or .str file.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get indata
            string filePath = null;
            if (!DA.GetData(0, ref filePath))
            {
                return;
            }

            if (filePath == null)
            {
                return;
            }

            if (_FileName.IsASCII(filePath))
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File path has special characters. This might cause problems.");


            if (Path.GetExtension(filePath) == ".str")
            {
                // Create FdScript
                var fdScript = FemDesign.Calculate.FdScript.ReadStr(filePath);

                // Run FdScript
                var app = new FemDesign.Calculate.Application();
                app.RunFdScript(fdScript, false, true, false, true);

                // Read model and results
                var strModel = Model.DeserializeFromFilePath(fdScript.StruxmlPath);
                DA.SetData(0, strModel);
            }
            else
            {
                //
                FemDesign.Model obj = FemDesign.Model.DeserializeFromFilePath(filePath);
                // return
                DA.SetData(0, obj);
            }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.ModelFromStruxml;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{B5904420-07A9-4F1E-BB1E-0282A962A70F}"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

    }
}