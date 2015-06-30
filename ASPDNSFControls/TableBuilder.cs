// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls
{   
    /// <summary>
    /// Summarize for creating table
    /// </summary>
    public class TableBuilder
    {
        #region Variable Declaration

        private Table _tblMain = null;
        private TableRow _currentRow = null;

        #endregion

        #region Constructor
        
        public TableBuilder(Table template)
        {
            _tblMain = template;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Creating new row
        /// </summary>
        /// <returns></returns>
        public TableRow NewRow()
        {
            TableRow row = new TableRow();
            _tblMain.Rows.Add(row);

            _currentRow = row;

            return row;
        }
        /// <summary>
        /// Adding table cell
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns></returns>
        public TableCell AddCell(Control control)
        {
            return AddCell(control, Unit.Percentage(100));
        }
        /// <summary>
        /// Adding table cell
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="width">width</param>
        /// <returns></returns>
        public TableCell AddCell(Control control, Unit width)
        {
            return AddCell(control, width, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }
        /// <summary>
        /// Adding table cell
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="width">width</param>
        /// <param name="alignHorizontal">Align horizontal</param>
        /// <param name="alignVertical">Align Vertical</param>
        /// <returns></returns>
        public TableCell AddCell(Control control, Unit width, HorizontalAlign alignHorizontal, VerticalAlign alignVertical)
        {
            if (_currentRow == null) throw new InvalidOperationException("NewRow must be called first!!!");

            TableCell tc = new TableCell();
            if (width != Unit.Empty)
            {
                tc.Width = width;
            }
            tc.HorizontalAlign = alignHorizontal;
            tc.VerticalAlign = alignVertical;
            tc.Controls.Add(control);

            _currentRow.Cells.Add(tc);

            return tc;
        }

        #endregion
    }
}
