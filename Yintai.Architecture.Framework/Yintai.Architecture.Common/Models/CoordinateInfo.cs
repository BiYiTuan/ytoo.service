using Yintai.Architecture.Common.Helper;

namespace Yintai.Architecture.Common.Models
{
    /// <summary>
    /// CLR Version: 4.0.30319.261
    /// NameSpace: Yintai.Architecture.Common.Models
    /// FileName: CoordinateInfo
    /// 
    /// Created at 4/6/2012 4:27:19 PM
    /// Description: ����
    /// </summary>
    public class CoordinateInfo
    {
        #region fields

        #endregion

        #region .ctor

        public CoordinateInfo(double longitude, double latitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;

            this.LatitudeDegree = CoordinatePositioningHelper.GetDegree(latitude);
            this.LongitudeDegree = CoordinatePositioningHelper.GetDegree(longitude);
        }




        #endregion

        #region properties

        /// <summary>
        /// ����
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// γ��
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// ���� �� ����
        /// </summary>
        public DegreeMinuteSecondInfo LongitudeDegree { get; set; }

        /// <summary>
        /// γ�� �� ����
        /// </summary>
        public DegreeMinuteSecondInfo LatitudeDegree { get; set; }

        #endregion

        #region methods

        #endregion
    }
}