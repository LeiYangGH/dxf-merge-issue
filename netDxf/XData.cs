
using System;
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf
{
    /// <summary>
    /// Represents the extended data information.
    /// </summary>
    /// <remarks>
    /// Do not store your own data under the ACAD application registry it is used by some entities to store special data,
    /// it might be overwritten when the file is saved. Instead, create a new application registry and store your data there.
    /// </remarks>
    public class XData :
        ICloneable
    {
        #region private fields

        private ApplicationRegistry appReg;
        private readonly List<XDataRecord> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initialize a new instance of the <c>XData</c> class .
        /// </summary>
        /// <param name="appReg">Name of the application associated with the list of extended data records.</param>
        public XData(ApplicationRegistry appReg)
        {
            this.appReg = appReg ?? throw new ArgumentNullException(nameof(appReg));
            this.xData = new List<XDataRecord>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the name of the application associated with the list of extended data records.
        /// </summary>
        public ApplicationRegistry ApplicationRegistry
        {
            get { return this.appReg; }
            internal set { this.appReg = value; }
        }

        /// <summary>
        /// Gets or sets the list of extended data records.
        /// </summary>
        /// <remarks>
        /// This list cannot contain a XDataRecord with a XDataCode of AppReg, this code is reserved to register the name of the application.
        /// Any record with this code will be omitted.
        /// </remarks>
        public List<XDataRecord> XDataRecord
        {
            get { return this.xData; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.appReg.Name;
        }

        #endregion

        #region implements ICloneable

        /// <summary>
        /// Creates a new XData that is a copy of the current instance.
        /// </summary>
        /// <returns>A new XData that is a copy of this instance.</returns>
        public object Clone()
        {
            XData xdata = new XData((ApplicationRegistry) this.appReg.Clone());
            foreach (XDataRecord record in this.xData)
            {
                xdata.XDataRecord.Add(new XDataRecord(record.Code, record.Value));
            }

            return xdata;
        }

        #endregion
    }
}