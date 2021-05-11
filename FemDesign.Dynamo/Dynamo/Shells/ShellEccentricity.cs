
#region dynamo
using Autodesk.DesignScript.Runtime;
#endregion

namespace FemDesign.Shells
{
    [IsVisibleInDynamoLibrary(false)]
    public partial class ShellEccentricity
    {
        /// <summary>
        /// Create a ShellEccentricity
        /// </summary>
        /// <remarks>Create</remarks>
        /// <param name="alignment">Alignment. "top"/"bottom"/"center".</param>
        /// <param name="eccentricity">Eccentricity.</param>
        /// <param name="eccentricityCalculation">Consider eccentricity in calculation? True/false.</param>
        /// <param name="eccentricityByCracking">Consider eccentricity caused by cracking in cracked section analysis? True/false.</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        public static ShellEccentricity Create(string alignment = "center", double eccentricity = 0, bool eccentricityCalculation = false, bool eccentricityByCracking = false)
        {
            return new ShellEccentricity(alignment, eccentricity, eccentricityCalculation, eccentricityByCracking);
        }

        /// <summary>
        /// Create a default ShellEccentricity.
        /// </summary>
        /// <remarks>Create</remarks>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        public static ShellEccentricity Default()
        {
            return GetDefault();
        }
    }
}