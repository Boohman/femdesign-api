﻿// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FemDesign.Grasshopper
{
    public class FemDesignConnectionComponent_OBSOLETE : GH_Component
    {
        private FemDesignConnection _connection;

        public FemDesignConnectionComponent_OBSOLETE() : base("FEM-Design.Connection", "Connection", "FemDesign.Connection has been released in the Beta stage and may have some bugs.", CategoryName.Name(), SubCategoryName.Cat8())
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FEM-Design dir", "FEM-Design dir",
                "Path to directory of FEM-Design installation.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;

            pManager.AddBooleanParameter("Minimized", "Minimized",
                "Should FEM-Design window be minimized?", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;

            pManager.AddTextParameter("OutputDir", "OutputDir",
                "The directory to save script files. If set to null, the files will be will be written to a temporary directory and deleted after.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "Connection", "FEM-Design application connection.", GH_ParamAccess.item);
        }
        public override void RemovedFromDocument(GH_Document document)
        {
            // Close FEM-Design connection when the component is removed
            _connection?.Dispose();
            _connection = null;

            base.RemovedFromDocument(document);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // TODO: Set version as global variable in FemDesign.Core

            string fd_installation_folder = $@"C:\Program Files\StruSoft\FEM-Design 22 Night Install\";
            DA.GetData("FEM-Design dir", ref fd_installation_folder);

            bool minimized = false;
            DA.GetData("Minimized", ref minimized);

            string outputDir = null;
            if (!DA.GetData("OutputDir", ref outputDir))
            {
                bool fileExist = OnPingDocument().IsFilePathDefined;
                if (!fileExist)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Save your .gh script or specfy a FilePath.");
                    return;
                }

                var _ghfileDir = System.IO.Path.GetDirectoryName(OnPingDocument().FilePath);
                System.IO.Directory.SetCurrentDirectory(_ghfileDir);
            }


            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Do not manually close the currently open instance of FEM-Design. \n If you want to close FEM-Design, delete this component.");



            if (_connection != null)
                _connection.Dispose();

            _connection = new FemDesignConnection(fd_installation_folder, minimized, outputDir: outputDir);

            DA.SetData("Connection", _connection);
        }

        protected override System.Drawing.Bitmap Icon => FemDesign.Properties.Resources.FEM_Connection;
        public override Guid ComponentGuid => new Guid("ae8824c3-26bf-42fd-9052-5f407b3540c1");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
    }
}

