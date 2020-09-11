using System.Collections.Generic;
using System.Xml.Serialization;

#region dynamo
using Autodesk.DesignScript.Runtime;
#endregion

namespace FemDesign.Loads
{
    [IsVisibleInDynamoLibrary(false)]
    [System.Serializable]
    public class LineTemperatureLoad: LoadBase
    {
        /// <summary>
        /// Edge defining the geometry of the load
        /// </summary>
        [XmlElement("edge", Order=1)]
        public Geometry.Edge Edge { get; set; }

        /// <summary>
        /// Direction of load.
        /// </summary>
        [XmlElement("direction", Order=2)]
        public Geometry.FdVector3d Direction { get; set; }

        /// <summary>
        /// Optional. Ambiguous what this does.
        /// </summary>
        /// <value></value>

        [XmlElement("normal", Order=3)]
        public Geometry.FdVector3d Normal { get; set; }

        /// <summary>
        /// Field
        /// </summary>
        [XmlElement("temperature", Order=4)]
        public List<TopBotLocationValue> _tempLocationValue;

        
        /// <summary>
        /// Top bottom value can be a list of 1 or 2 items. 1 item defines a uniform line load, 2 items defines a variable line load.
        /// </summary>
        [XmlIgnore]
        public List<TopBotLocationValue> TempLocationValue
        {
            get
            {
                return this._tempLocationValue;
            }
            set
            {
                if (value.Count == 2)
                {
                    this._tempLocationValue = value;
                }
                else
                {
                    throw new System.ArgumentException($"Length of list is: {value.Count}, expected 2");
                }
            }
        }

        /// <summary>
        /// Parameterless constructor for serialization
        /// </summary>
        private LineTemperatureLoad()
        {

        }

        /// <summary>
        /// Construct a uniform or variable line temperature load
        /// </summary>
        /// <param name="edge">Underlying edge of line load. Line or Arc.</param>
        /// <param name="direction">Directio of load.</param>
        /// <param name="topBotLocVal">1 or 2 top bottom location values</param>
        public LineTemperatureLoad(Geometry.Edge edge, Geometry.FdVector3d direction, List<TopBotLocationValue> topBotLocVals, LoadCase _loadCase, string _comment)
        {
            this.EntityCreated();
            this.Edge = edge;
            this.Direction = direction;
            this.TempLocationValue = topBotLocVals;
            this.loadCase = _loadCase.guid;
            this.comment = _comment;
        }

        #region dynamo
        /// <summary>
        /// Define a line temperature load
        /// </summary>
        /// <param name="curve">Curve of line temperature load</param>
        /// <param name="direction">Direction of load</param>
        /// <param name="topBottomLocationValues">Top bottom location value</param>
        /// <param name="loadCase">Load case of load</param>
        /// <param name="comments">Comment of load</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        public static LineTemperatureLoad Define(Autodesk.DesignScript.Geometry.Curve curve, [DefaultArgument("Autodesk.DesignScript.Geometry.Vector.ByCoordinates(0,0,1)")] Autodesk.DesignScript.Geometry.Vector direction, List<TopBotLocationValue> topBottomLocationValues, LoadCase loadCase, string comments = "")
        {
            // convert geometry
            Geometry.Edge edge = Geometry.Edge.FromDynamoLineOrArc1(curve);
            Geometry.FdVector3d v = Geometry.FdVector3d.FromDynamo(direction);

            // return
            return new LineTemperatureLoad(edge, v, topBottomLocationValues, loadCase, comments);
        }
        #endregion
    }
}