using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MySoft.Web.UI.Controls
{

    /********************************************************
      Class DataCalendarItem
        - serves as the container for a single calendar entry,
          allowing for databinding syntax like the following
          to be used in the .aspx page:
          
            <%# Container.DataItem("MyField") %>            
     ********************************************************/

    public class DataCalendarItem : Control, INamingContainer
    {

        private DataRow _dataItem;

        public DataCalendarItem(DataRow dr)
        {
            _dataItem = dr;
        }

        // because the source data will be a DataTable
        // object, it makes sense for our DataItem
        // property to return a DataRow object
        // (i.e. a single item in the data source
        //  corresponds to a single row of data)
        public DataRow DataItem
        {
            get { return _dataItem; }
            set { _dataItem = value; }
        }
    }


    /********************************************************
      Class DataCalendar
        - subclass of the ASP.NET Calendar control for
          displaying events from a DataTable with support
          for templates
     ********************************************************/

    public class DataCalendar : Calendar, INamingContainer
    {

        private object _dataSource;
        private string _dataMember;
        private string _dayField;
        private ITemplate _itemTemplate;
        private ITemplate _noEventsTemplate;
        private TableItemStyle _dayWithEventsStyle;
        private DataTable _dtSource;

        // Support either a DataSet or DataTable object
        // for the DataSource property
        public object DataSource
        {
            get { return _dataSource; }
            set
            {
                if (value is DataTable || value is DataSet)
                    _dataSource = value;
                else
                    throw new WebException("The DataSource property of the DataCalendar control" +
                                        " must be a DataTable or DataSet object");
            }
        }

        // If a DataSet is supplied for DataSource,
        // use this property to determine which
        // DataTable within the DataSet should
        // be used; if DataMember is not supplied,
        // the first table in the DataSet will
        // be used.
        public string DataMember
        {
            get { return _dataMember; }
            set { _dataMember = value; }
        }


        // Specify the name of the field within
        // the source DataTable that contains
        // a DateTime value for displaying in the
        // calendar.
        public string DayField
        {
            get { return _dayField; }
            set { _dayField = value; }
        }

        public TableItemStyle DayWithEventsStyle
        {
            get { return _dayWithEventsStyle; }
            set { _dayWithEventsStyle = value; }
        }

        [TemplateContainer(typeof(DataCalendarItem))]
        public ITemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set { _itemTemplate = value; }
        }


        [TemplateContainer(typeof(DataCalendarItem))]
        public ITemplate NoEventsTemplate
        {
            get { return _noEventsTemplate; }
            set { _noEventsTemplate = value; }
        }


        // Constructor    
        public DataCalendar()
            : base()
        {
            // since this control will be used for displaying
            // events, set these properties as a default
            this.SelectionMode = CalendarSelectionMode.None;
            this.ShowGridLines = true;
        }

        private void SetupCalendarItem(TableCell cell, DataRow r, ITemplate t)
        {
            // given a calendar cell and a datarow, set up the
            // templated item and resolve data binding syntax
            // in the template
            DataCalendarItem dti = new DataCalendarItem(r);
            t.InstantiateIn(dti);
            dti.DataBind();
            cell.Controls.Add(dti);
        }


        protected override void OnDayRender(TableCell cell, CalendarDay day)
        {
            // _dtSource was already set by the Render method            
            if (_dtSource != null)
            {

                // We have the data source as a DataTable now;                
                // filter the records in the DataTable for the given day;
                // force the date format to be MM/dd/yyyy
                // to ensure compatibility with RowFilter
                // date expression syntax (#date#).
                // Also, take the possibility of time
                // values into account by specifying
                // a date range, to include the full day
                DataView dv = new DataView(_dtSource);
                dv.RowFilter = string.Format(
                   "{0} >= #{1}# and {0} < #{2}#",
                   this.DayField,
                   day.Date.ToString("MM/dd/yyyy"),
                   day.Date.AddDays(1).ToString("MM/dd/yyyy")
                );

                // are there events on this day?
                if (dv.Count > 0)
                {
                    // there are events on this day; if indicated, 
                    // apply the DayWithEventsStyle to the table cell
                    if (this.DayWithEventsStyle != null)
                        cell.ApplyStyle(this.DayWithEventsStyle);

                    // for each event on this day apply the
                    // ItemTemplate, with data bound to the item's row
                    // from the data source
                    if (this.ItemTemplate != null)
                        for (int i = 0; i < dv.Count; i++)
                        {
                            SetupCalendarItem(cell, dv[i].Row, this.ItemTemplate);
                        }

                }
                else
                {
                    // no events this day;
                    if (this.NoEventsTemplate != null)
                        SetupCalendarItem(cell, null, this.NoEventsTemplate);

                }

            }

            // call the base render method too
            base.OnDayRender(cell, day);

        }

        protected override void Render(HtmlTextWriter html)
        {
            _dtSource = null;

            if (this.DataSource != null && this.DayField != null)
            {
                // determine if the datasource is a DataSet or DataTable
                if (this.DataSource is DataTable)
                    _dtSource = (DataTable)this.DataSource;
                if (this.DataSource is DataSet)
                {
                    DataSet ds = (DataSet)this.DataSource;
                    if (this.DataMember == null || this.DataMember == "")
                        // if data member isn't supplied, default to the first table
                        _dtSource = ds.Tables[0];
                    else
                        // if data member is supplied, use it
                        _dtSource = ds.Tables[this.DataMember];
                }
                // throw an exception if there is a problem with the data source
                if (_dtSource == null)
                    throw new WebException("Error finding the DataSource.  Please check " +
                                        " the DataSource and DataMember properties.");
            }

            // call the base Calendar's Render method
            // allowing OnDayRender() to be executed
            base.Render(html);
        }


    }

}
