﻿#define col_class
#define crt_tables
#define init_datatable_cols
#define use_cmd_params
#define use_single_qry
#define use_progress_dlg

using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System;
using System.Data.SQLite;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Threading.Tasks;

namespace test_binding
{
    /// <summary>
    /// table info
    /// + fields
    /// + fileds type
    /// + alias
    /// </summary>
    [DataContract(Name = "TableInfo")]
    public class lTableInfo
    {
        //#define col_class
#if col_class
        [DataContract(Name = "ColInfo")]
        public class lColInfo
        {
            [DataContract(Name = "ColType")]
            public enum lColType
            {
                [EnumMember]
                text,
                [EnumMember]
                dateTime,
                [EnumMember]
                num,
                [EnumMember]
                currency
            };
            [DataMember(Name = "field", EmitDefaultValue = false)]
            public string m_field;
            [DataMember(Name = "alias", EmitDefaultValue = false)]
            public string m_alias;
            [DataMember(Name = "lookupTbl", EmitDefaultValue = false)]
            public string m_lookupTbl;
            [DataMember(Name = "type", EmitDefaultValue = false)]
            public lColType m_type;

            public lDataSync m_lookupData;
            private void init(string field, string alias, lColType type, string lookupTbl)
            {
                m_lookupTbl = lookupTbl;
                m_field = field;
                m_alias = alias;
                m_type = type;
            }
            public lColInfo(string field, string alias, lColType type, string lookupTbl)
            {
                init(field, alias, type, lookupTbl);
            }
            public lColInfo(string field, string alias, lColType type)
            {
                init(field, alias, type, null);
            }
        };

        [DataMember(Name = "cols", EmitDefaultValue = false)]
        public lColInfo[] m_cols;
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string m_tblName;
        [DataMember(Name = "alias", EmitDefaultValue = false)]
        public string m_tblAlias;
        [DataMember(Name = "crtSql", EmitDefaultValue = false)]
        public string m_crtQry;

        public virtual void LoadData()
        {
            foreach (lColInfo colInfo in m_cols)
            {
                if (colInfo.m_lookupTbl != null)
                {
                    colInfo.m_lookupData = appConfig.s_contentProvider.CreateDataSync(colInfo.m_lookupTbl);
                    colInfo.m_lookupData.LoadData();
                }
            }
        }
#else
    public struct lColInfo
    {
        public enum lColType
        {
            text,
            dateTime,
            num
        };
        public string m_field;
        public string m_alias;
        public lColType m_type;
    };
    public virtual lColInfo[] getColsInfo() { return null; }
#endif
        public int getColIndex(string colName)
        {
            int i = 0;
            foreach (lColInfo col in m_cols)
            {
                if (col.m_field == colName)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }

    [DataContract(Name = "ReceiptsTblInfo")]
    public class lReceiptsTblInfo : lTableInfo
    {
#if col_class
        public lReceiptsTblInfo()
        {
            m_tblName = "receipts";
            m_tblAlias = "Bảng Thu";
            m_crtQry = "CREATE TABLE if not exists  receipts("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "date datetime,"
            + "receipt_number char(31),"
            + "name char(31),"
            + "content text,"
            + "amount INTEGER,"
            + "note text"
            + ")";
            m_cols = new lColInfo[] {
                   new lColInfo( "ID","ID", lColInfo.lColType.num),
                   new lColInfo( "date","Ngày Tháng", lColInfo.lColType.dateTime),
                   new lColInfo( "receipt_number","Mã Phiếu Thu", lColInfo.lColType.text),
                   new lColInfo( "name","Họ tên", lColInfo.lColType.text),
                   new lColInfo( "content","Nội dung", lColInfo.lColType.text, "receipts_content"),
                   new lColInfo( "amount","Số tiền", lColInfo.lColType.currency),
                   new lColInfo( "note","Ghi chú", lColInfo.lColType.text),
                };
        }
#else
    static lColInfo[] m_cols = new lColInfo[] {
               new lColInfo() {m_field = "ID", m_alias = "ID", m_type = lColInfo.lColType.num }
            };
    public override lColInfo[] getColsInfo()
    {
        return m_cols;
    }
#endif

    };
    [DataContract(Name = "InternalPaymentTblInfo")]
    public class lInternalPaymentTblInfo : lTableInfo
    {
        public lInternalPaymentTblInfo()
        {
            m_tblName = "internal_payment";
            m_tblAlias = "Chi Nội Chúng";
            m_crtQry = "CREATE TABLE if not exists internal_payment("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "date datetime,"
            + "payment_number char(31),"
            + "name char(31),"
            + "content text,"
            + "group_name char(31),"
            + "advance_payment INTEGER,"
            + "reimbursement INTEGER,"
            + "actually_spent INTEGER,"
            + "note text"
            + ")";
            m_cols = new lColInfo[] {
                   new lColInfo( "ID","ID", lColInfo.lColType.num),
                   new lColInfo( "date","Ngày Tháng", lColInfo.lColType.dateTime),
                   new lColInfo( "payment_number","Mã Phiếu Chi", lColInfo.lColType.text),
                   new lColInfo( "name","Họ Tên", lColInfo.lColType.text),
                   new lColInfo( "content","Nội dung", lColInfo.lColType.text),
                   new lColInfo( "group_name","Thuộc ban", lColInfo.lColType.text, "group_name"),
                   new lColInfo( "advance_payment","Tạm ứng", lColInfo.lColType.currency),
                   new lColInfo( "reimbursement","Hoàn ứng", lColInfo.lColType.currency),
                   new lColInfo( "actually_spent","Thực chi", lColInfo.lColType.currency),
                   new lColInfo( "note","Ghi Chú", lColInfo.lColType.text),
                };
        }
    };
    [DataContract(Name = "ExternalPaymentTblInfo")]
    public class lExternalPaymentTblInfo : lTableInfo
    {
        public lExternalPaymentTblInfo()
        {
            m_tblName = "external_payment";
            m_tblAlias = "Chi Ngoại Chúng";
            m_crtQry = "CREATE TABLE if not exists external_payment("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "date datetime,"
            + "payment_number char(31),"
            + "name char(31),"
            + "content text,"
            + "group_name char(31),"
            + "spent INTEGER,"
            + "note text"
            + ")";
            m_cols = new lColInfo[] {
                   new lColInfo( "ID","ID", lColInfo.lColType.num),
                   new lColInfo( "date","Ngày Tháng", lColInfo.lColType.dateTime),
                   new lColInfo( "payment_number","Mã Phiếu Chi", lColInfo.lColType.text),
                   new lColInfo( "name","Họ Tên", lColInfo.lColType.text),
                   new lColInfo( "content","Nội dung", lColInfo.lColType.text),
                   new lColInfo( "group_name","Thuộc ban", lColInfo.lColType.text, "group_name"),
                   new lColInfo( "spent","Số tiền", lColInfo.lColType.currency),
                   new lColInfo( "note","Ghi Chú", lColInfo.lColType.text),
                };
        }
    };
    [DataContract(Name = "SalaryTblInfo")]
    public class lSalaryTblInfo : lTableInfo
    {
        public lSalaryTblInfo()
        {
            m_tblName = "salary";
            m_tblAlias = "Bảng Lương";
            m_crtQry = "CREATE TABLE if not exists salary("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "month INTEGER,"
            + "date datetime,"
            + "payment_number char(31),"
            + "name char(31),"
            + "group_name char(31),"
            + "content text,"
            + "salary INTEGER,"
            + "note text"
            + ")";
            m_cols = new lColInfo[] {
                   new lColInfo( "ID","ID", lColInfo.lColType.num),
                   new lColInfo( "month","Tháng(1...12)", lColInfo.lColType.num),
                   new lColInfo( "date","Ngày Tháng", lColInfo.lColType.dateTime),
                   new lColInfo( "payment_number","Mã Phiếu Chi", lColInfo.lColType.text),
                   new lColInfo( "name","Họ Tên", lColInfo.lColType.text),
                   new lColInfo( "group_name","Thuộc ban", lColInfo.lColType.text, "group_name"),
                   new lColInfo( "content","Nội dung", lColInfo.lColType.text),
                   new lColInfo( "salary","Số tiền", lColInfo.lColType.currency),
                   new lColInfo( "note","Ghi Chú", lColInfo.lColType.text),
                };
        }
    };
    [DataContract(Name = "lGroupNameTblInfo")]
    public class lGroupNameTblInfo : lTableInfo
    {
        public lGroupNameTblInfo()
        {
            m_tblName = "group_name";
            m_tblAlias = "Các ban";
            m_crtQry = "CREATE TABLE if not exists group_name("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT, "
                + "name nchar(31))";
            m_cols = new lColInfo[] {
                   new lColInfo( "ID","ID", lColInfo.lColType.num),
                   new lColInfo( "name","Các ban", lColInfo.lColType.text)
                };
        }
    };
    [DataContract(Name = "lReceiptsContentTblInfo")]
    public class lReceiptsContentTblInfo : lTableInfo
    {
        public lReceiptsContentTblInfo()
        {
            m_tblName = "receipts_content";
            m_tblAlias = "Nội dung chi";
            m_crtQry = "CREATE TABLE if not exists receipts_content("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + " content nchar(31))";
            m_cols = new lColInfo[] {
                   new lColInfo( "ID","ID", lColInfo.lColType.num),
                   new lColInfo( "content","Nội dung chi", lColInfo.lColType.text)
                };
        }
    };
    [DataContract(Name = "lReceiptsViewInfo")]
    public class lReceiptsViewInfo : lTableInfo
    {
        public lReceiptsViewInfo()
        {
            m_tblName = "v_receipts";
            m_crtQry = "CREATE VIEW if not exists v_receipts  as  "
                + "select content, amount/1000 as amount, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from receipts "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
        }
    };
    [DataContract(Name = "lInterPaymentViewInfo")]
    public class lInterPaymentViewInfo : lTableInfo
    {
        public lInterPaymentViewInfo()
        {
            m_tblName = "v_internal_payment";
            m_crtQry = "CREATE VIEW if not exists v_internal_payment as "
                + "select group_name, actually_spent/1000 as actually_spent, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from internal_payment "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
        }
    };
    [DataContract(Name = "lExterPaymentViewInfo")]
    public class lExterPaymentViewInfo : lTableInfo
    {
        public lExterPaymentViewInfo()
        {
            m_tblName = "v_external_payment";
            m_crtQry = "CREATE VIEW if not exists v_external_payment as "
                + "select group_name, spent/1000 as spent, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from external_payment "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
        }
    };
    [DataContract(Name = "lSalaryViewInfo")]
    public class lSalaryViewInfo : lTableInfo
    {
        public lSalaryViewInfo()
        {
            m_tblName = "v_salary";
            m_crtQry = "CREATE VIEW if not exists v_salary as "
                + "select group_name, salary/1000 as salary, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from salary "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
        }
    };

    public class lContentProvider
    {
        protected lContentProvider()
        {
            m_dataSyncs = new Dictionary<string, lDataSync>();
            m_dataContents = new Dictionary<string, lDataContent>();
        }

        protected Form m_form;
        private Dictionary<string, lDataSync> m_dataSyncs;
        private Dictionary<string, lDataContent> m_dataContents;

        protected virtual lDataContent newDataContent(string tblName) { return null; }
        protected virtual lDataSync newDataSync(string tblName)
        {
            lDataContent dataContent = CreateDataContent(tblName);
            lDataSync dataSync = new lDataSync(dataContent);
            return dataSync;
        }

        public virtual DataTable GetData(string qry) { return null; }
        public lDataContent CreateDataContent(string tblName)
        {
            lDataContent dataContent = newDataContent(tblName);
            if (!m_dataContents.ContainsKey(tblName))
            {
                lDataContent data = newDataContent(tblName);
                m_dataContents.Add(tblName, data);
                return data;
            }
            else
            {
                return m_dataContents[tblName];
            }
        }
        public lDataSync CreateDataSync(string tblName)
        {
            if (!m_dataSyncs.ContainsKey(tblName))
            {
                lDataContent dataContent = CreateDataContent(tblName);
                lDataSync dataSync = new lDataSync(dataContent);
                m_dataSyncs.Add(tblName, dataSync);
                return dataSync;
            }
            else
            {
                return m_dataSyncs[tblName];
            }
        }
    }
    public class lSqlContentProvider : lContentProvider, IDisposable
    {
        static lSqlContentProvider m_instance;
        public static lContentProvider getInstance(Form parent)
        {
            if (m_instance == null)
            {
                m_instance = new lSqlContentProvider();
                m_instance.m_form = parent;
            }
            return m_instance;
        }

        lSqlContentProvider() : base()
        {
            string cnnStr = "Data Source=DESKTOP-GOEF1DS\\SQLEXPRESS;Initial Catalog=accounting;Integrated Security=true";
            //string cnnStr = s_config.m_dbSchema.m_cnnStr;
            m_cnn = new SqlConnection(cnnStr);
            m_cnn.Open();
        }

        private SqlConnection m_cnn;

        protected override lDataContent newDataContent(string tblName)
        {
            lSqlDataContent data = new lSqlDataContent(tblName, m_cnn);
            return data;
        }

        public override DataTable GetData(string qry)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = new SqlCommand(qry, m_cnn);
            // Populate a new data table and bind it to the BindingSource.
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            return table;
        }

        public void Dispose()
        {
            m_cnn.Dispose();
        }
    }
    public class lSQLiteContentProvider : lContentProvider, IDisposable
    {
        static lSQLiteContentProvider m_instance;
        public static lContentProvider getInstance(Form parent)
        {
            if (m_instance == null)
            {
                m_instance = new lSQLiteContentProvider();
                m_instance.m_form = parent;
            }
            return m_instance;
        }

        lSQLiteContentProvider() : base()
        {
            //string dbPath = "test.db";
            string dbPath = appConfig.s_config.m_dbSchema.m_cnnStr;
            bool bCrtTbls = false;
            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                bCrtTbls = true;
            }
            m_cnn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", dbPath));
            m_cnn.Open();
#if crt_tables
            if (bCrtTbls)
            {
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = m_cnn;
                List<string> sqls = new List<string>();
                foreach (lTableInfo tbl in appConfig.s_config.m_dbSchema.m_tables)
                {
                    sqls.Add(tbl.m_crtQry);
                }
                foreach (lTableInfo view in appConfig.s_config.m_dbSchema.m_views)
                {
                    sqls.Add(view.m_crtQry);
                }
                foreach (var sql in sqls)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
#endif  //crt_tables
        }

        private SQLiteConnection m_cnn;

        public override DataTable GetData(string qry)
        {
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter();
            dataAdapter.SelectCommand = new SQLiteCommand(qry, m_cnn);
            // Populate a new data table and bind it to the BindingSource.
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            return table;
        }

        protected override lDataContent newDataContent(string tblName)
        {
            lSQLiteDataContent data = new lSQLiteDataContent(tblName, m_cnn);
            data.m_form = m_form;
            return data;
        }

        public void Dispose()
        {
            m_cnn.Dispose();
        }
    }

    [DataContract(Name = "DbSchema")]
    public class lDbSchema
    {
        [DataMember(Name = "cnnStr")]
        public string m_cnnStr;
#if crt_qry
            [DataMember(Name ="crtTableSqls")]
            public List<string> m_crtTableSqls;
            [DataMember(Name = "crtViewSqls")]
            public List<string> m_crtViewSqls;
#endif
        [DataMember(Name = "tables")]
        public List<lTableInfo> m_tables;
        [DataMember(Name = "views")]
        public List<lTableInfo> m_views;

        public lDbSchema()
        {
        }
    }
    [DataContract(Name = "SQLiteDbSchema")]
    public class lSQLiteDbSchema : lDbSchema
    {
        public lSQLiteDbSchema()
        {
            m_cnnStr = @"..\..\appData.db";
#if crt_qry
                m_crtTableSqls = new List<string> {
                    "CREATE TABLE if not exists  receipts("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "receipt_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "amount INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists internal_payment("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "group_name char(31),"
                    + "advance_payment INTEGER,"
                    + "reimbursement INTEGER,"
                    + "actually_spent INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists external_payment("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "group_name char(31),"
                    + "spent INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists salary("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "month INTEGER,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "group_name char(31),"
                    + "content text,"
                    + "salary INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists receipts_content(ID INTEGER PRIMARY KEY AUTOINCREMENT, content nchar(31));",
                    "CREATE TABLE if not exists group_name(ID INTEGER PRIMARY KEY AUTOINCREMENT, name nchar(31));",
            };
                m_crtViewSqls = new List<string> {
                    "CREATE VIEW if not exists v_receipts "
                    + " as "
                    + " select content, amount, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr "
                    + " from receipts"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;" ,
                    " CREATE VIEW if not exists v_internal_payment"
                    + " as"
                    + " select group_name, actually_spent, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from internal_payment"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",

                    "CREATE VIEW if not exists v_external_payment"
                    + " as"
                    + " select group_name, spent, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from external_payment"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",
                    "CREATE VIEW if not exists v_salary"
                    + " as"
                    + " select group_name, salary, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from salary"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",
                };
#endif  //crt_qry
            m_tables = new List<lTableInfo>() {
                    new lReceiptsTblInfo(),
                    new lInternalPaymentTblInfo(),
                    new lExternalPaymentTblInfo(),
                    new lSalaryTblInfo(),
                    new lReceiptsContentTblInfo(),
                    new lGroupNameTblInfo()
                };
            m_views = new List<lTableInfo>() {
                    new lReceiptsViewInfo(),
                    new lInterPaymentViewInfo(),
                    new lExterPaymentViewInfo(),
                    new lSalaryViewInfo()
                };
        }
    }

    /// <summary>
    /// data content
    /// + getdata()
    ///     return databinding
    /// + reload()
    /// + submit()
    /// </summary>
    public class lDataContent : myCursor, IDisposable
    {
        #region fetch_data
        public Form m_form;
        public DataTable m_dataTable { get; private set; }

        public class FillTableCompletedEventArgs : EventArgs
        {
            public Int64 Sum { get; set; }
            public DateTime TimeComplete { get; set; }
        }

        protected virtual void OnFillTableCompleted(FillTableCompletedEventArgs e)
        {
            EventHandler< FillTableCompletedEventArgs> handler = FillTableCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FillTableCompletedEventArgs> FillTableCompleted;

        protected virtual Int64 getMaxRowId() { return 0; }
        protected virtual Int64 getRowCount() { return 0; }
        protected virtual void fillTable() { throw new NotImplementedException(); }
        void invokeFetchLargeData()
        {
            var task = m_form.BeginInvoke(new noParamDelegate(fetchLargeData));
            //lPrgDlg prg = new lPrgDlg();
            ProgressDlg prg = new ProgressDlg();
            prg.m_param = task;
            prg.m_cursor = this;
            prg.m_maxRowid = getMaxRowId();
            prg.m_scale = 1000;
            prg.m_descr = "Getting data ...";
            Task tMor = Task.Run(() =>
            {
                prg.ShowDialog();
                prg.Dispose();
            });
        }
        void fetchLargeData()
        {
            using (new myElapsed())
            {
                DataTable tbl = m_dataTable;

                tbl.Clear();
                tbl.Locale = System.Globalization.CultureInfo.InvariantCulture;

                tbl.RowChanged += M_tbl_RowChanged;
                m_lastId = 0;

                fillTable();

                tbl.RowChanged -= M_tbl_RowChanged;
            }

            OnFillTableCompleted(new FillTableCompletedEventArgs());

            if (m_refresher != null)
                m_refresher.Refresh();
        }
#if use_single_qry
        Int64 m_lastId = 0;
        private void M_tbl_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            //Debug.WriteLine("{0}.M_tbl_RowChanged {1}", this, e.Row[0]);
            m_lastId = (Int64)e.Row[0];
        }
        Int64 myCursor.getPos() { return m_lastId; }
#endif
        delegate void noParamDelegate();
        protected virtual void fetchData()
        {
            if (getRowCount() > 1000)
                invokeFetchLargeData();
            else
                fetchSmallData();
        }
        void fetchSmallData()
        {
            DataTable table = m_dataTable;
            table.Clear();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            fillTable();

            OnFillTableCompleted(new FillTableCompletedEventArgs());

            if (m_refresher != null)
                m_refresher.Refresh();
        }
        #endregion

        public BindingSource m_bindingSource { get; private set; }
        protected string m_table;
        public IRefresher m_refresher;
        public lDataContent()
        {
            m_dataTable = new DataTable();
            m_bindingSource = new BindingSource();
            m_bindingSource.DataSource = m_dataTable;
        }
        protected void init()
        {
            if (m_dataTable.Columns.Count == 0)
            {
                lTableInfo tbl = appConfig.s_config.getTable(m_table);
                if (tbl == null) return;
                foreach (var col in tbl.m_cols)
                {
                    DataColumn dc = m_dataTable.Columns.Add(col.m_field);
                    switch (col.m_type)
                    {
                        case lTableInfo.lColInfo.lColType.num:
                        case lTableInfo.lColInfo.lColType.currency:
                            dc.DataType = typeof(Int64);
                            break;
                        case lTableInfo.lColInfo.lColType.dateTime:
                            dc.DataType = typeof(DateTime);
                            break;
                    }
                }
            }
        }
#if !use_cmd_params
            public virtual void Search(string exprs)
            {
                string sql = string.Format("select * from {0} ", m_table);
                if (exprs != null)
                {
                    sql += " where " + exprs;
                }
                GetData(sql);
            }
#endif
#if use_cmd_params
        public virtual void Search(List<string> exprs, List<lSearchParam> srchParams) { throw new NotImplementedException(); }
#endif
        bool m_changed = true;
        public virtual void Load(bool bLoadFullData) { throw new NotFiniteNumberException(); }
        public virtual void Load() { if (m_changed) { Reload(); } }
        public virtual void Reload() { m_changed = false; }
        public virtual void Submit() { m_changed = false; }
        protected virtual void GetData(string sql) { throw new NotImplementedException(); }

        public void Dispose()
        {
            m_bindingSource.Dispose();
            m_dataTable.Dispose();
        }
    }
    public class lSQLiteDataContent : lDataContent, IDisposable
    {
        private readonly SQLiteConnection m_cnn;
        private SQLiteDataAdapter m_dataAdapter;

        public lSQLiteDataContent(string tblName, SQLiteConnection cnn)
            : base()
        {
            m_table = tblName;
            m_cnn = cnn;
            m_dataAdapter = new SQLiteDataAdapter();
            m_dataAdapter.SelectCommand = new SQLiteCommand(selectLast(100), cnn);
            m_dataAdapter.RowUpdated += M_dataAdapter_RowUpdated;
#if init_datatable_cols
            init();
#endif
        }

        string selectLast(int count)
        {
            return string.Format("select * from {0} "
                + " where "
                + " id in (SELECT id from {0} order by id desc limit {1})",
                m_table, count);
        }
#if !use_cmd_params
            public override void Search(string exprs)
            {
                string sql = string.Format("select * from {0} ", m_table);
                if (exprs != null)
                {
                    sql += " where " + exprs;
                }
                else
                {
                    sql = selectLast100();
                }
                GetData(sql);
            }
#endif
#if use_cmd_params
        public override void Search(List<string> exprs, List<lSearchParam> srchParams)
        {
            SQLiteCommand selectCommand;
            string sql = string.Format("select * from {0} ", m_table);
            if (exprs.Count > 0)
            {
                sql += " where " + string.Join(" and ", exprs);
                selectCommand = new SQLiteCommand(sql, m_cnn);
                foreach (var param in srchParams)
                {
                    selectCommand.Parameters.AddWithValue(param.key, param.val);
                }
            }
            else
            {
                selectCommand = new SQLiteCommand(selectLast(100), m_cnn);
            }
            GetData(selectCommand);
        }
#endif
        public override void Load(bool bLoadFullData)
        {
            if (bLoadFullData)
            {
                m_dataAdapter.SelectCommand.CommandText = string.Format("select * from {0}", m_table);
            }
            Reload();
        }
        public override void Reload()
        {
            base.Reload();
            GetData(m_dataAdapter.SelectCommand);
        }
        public override void Submit()
        {
            base.Submit();
            using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder(m_dataAdapter))
            {
                DataTable dt = m_dataTable;
                if (dt != null)
                {
                    m_dataAdapter.UpdateCommand = builder.GetUpdateCommand();
                    m_dataAdapter.Update(dt);
                }
            }
        }

        private void M_dataAdapter_RowUpdated(object sender, System.Data.Common.RowUpdatedEventArgs e)
        {
            //udpate row id
            if (e.StatementType == StatementType.Insert)
            {
                Int64 rowid = m_cnn.LastInsertRowId;
                e.Row[0] = rowid;
                Debug.WriteLine("M_dataAdapter_RowUpdated {0}", e.Row[0]);
            }
        }

        protected override void GetData(string selectStr)
        {
            SQLiteCommand selectCommand = new SQLiteCommand(selectStr, m_cnn);
            GetData(selectCommand);
        }
        private void GetData(SQLiteCommand selectCommand)
        {
            Debug.WriteLine("{0}.GetData {1}", this, selectCommand.CommandText);
            m_dataAdapter.SelectCommand = selectCommand;
            // Populate a new data table and bind it to the BindingSource.
            fetchData();
        }

        public new void Dispose()
        {
            m_dataAdapter.Dispose();
        }

        #region fetch_data
        Int64 qryOne(string qry)
        {
            SQLiteConnection cnn = m_cnn;
            SQLiteCommand cmd = new SQLiteCommand(qry, cnn);
            var ret = cmd.ExecuteScalar();
            return (Int64)ret;
        }
        protected override Int64 getMaxRowId()
        {
            return qryOne(string.Format("select max(id) from {0}", m_table));
        }
        protected override Int64 getRowCount()
        {
            return qryOne(string.Format("select count(*) from {0}", m_table));
        }
        protected override void fillTable()
        {
            m_dataAdapter.Fill(m_dataTable);
        }
        #endregion
    }
    public class lSqlDataContent : lDataContent
    {
        private SqlConnection m_cnn;
        private SqlDataAdapter m_dataAdapter;

        public lSqlDataContent(string tblName, SqlConnection cnn)
            : base()
        {
            m_table = tblName;
            m_cnn = cnn;
            m_dataAdapter = new SqlDataAdapter();
            m_dataAdapter.SelectCommand = new SqlCommand(string.Format("select * from {0}", tblName), cnn);
#if init_datatable_cols
            init();
#endif
        }

#if !use_cmd_params
            public override void Search(string exprs)
            {
                string sql = string.Format("select * from {0} ", m_table);
                if (exprs != null)
                {
                    sql += " where " + exprs;
                }
                GetData(sql);
            }
#endif
#if use_cmd_params
        public override void Search(List<string> exprs, List<lSearchParam> srchParams)
        {
            string sql = string.Format("select * from {0} ", m_table);

            if (exprs.Count > 0)
            {
                sql += " where " + string.Join(" and ", exprs);
                SqlCommand selectCommand = new SqlCommand(sql, m_cnn);
                foreach (var param in srchParams)
                {
                    var p = selectCommand.Parameters.AddWithValue(param.key, param.val);
                    if (param.type == DbType.String) { p.DbType = DbType.String; }
                }
                GetData(selectCommand);
            }
            else
            {
                GetData(sql);
            }
        }
#endif
        public override void Reload()
        {
            base.Reload();
            GetData(m_dataAdapter.SelectCommand);
        }
        public override void Submit()
        {
            base.Submit();
            using (SqlCommandBuilder builder = new SqlCommandBuilder(m_dataAdapter))
            {
                DataTable dt = (DataTable)m_bindingSource.DataSource;
                if (dt != null)
                {
                    m_dataAdapter.UpdateCommand = builder.GetUpdateCommand();
                    m_dataAdapter.Update(dt);
                }
            }
        }
        protected override void GetData(string selectStr)
        {
            SqlCommand selectCommand = new SqlCommand(selectStr, m_cnn);
            GetData(selectCommand);
        }
        private void GetData(SqlCommand selectCommand)
        {
            m_dataAdapter.SelectCommand = selectCommand;
            // Populate a new data table and bind it to the BindingSource.
            fetchData();
        }
    }

    public interface IRefresher
    {
        void Refresh();
    }
    public class lDataSync : IRefresher
    {
        private lDataContent m_data;
        public AutoCompleteStringCollection m_colls;
        public Dictionary<string, string> m_maps;
        public lDataSync(lDataContent data)
        {
            m_data = data;
            m_data.m_refresher = this;
        }

        static Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"[áàảãạ]", "a"  },
                {"[ăằắẵẳặ]", "a"},
                {"[âầấẫẩậ]", "a"},
                {"[đ]", "d"     },
                {"[éèẻẽẹ]", "e" },
                {"[êềếễểệ]", "e"},
                {"[íìĩỉị]", "i" },
                {"[òóỏõọ]",  "o"},
                {"[ồôỗốộổ]",  "o"},
                {"[ơờớỡởợ]", "o"},
                {"[úùủũụ]", "u" },
                {"[ừưữứửự]", "u"},
            };
        static string genKey(string value)
        {
            string key = value.ToLower();
            foreach (var i in dict)
            {
                key = Regex.Replace(key, i.Key, i.Value);
            }
            return key;
        }
        public void Refresh()
        {
            m_colls = new AutoCompleteStringCollection();
            m_maps = new Dictionary<string, string>();
            DataTable tbl = m_dataSource;
            foreach (DataRow row in tbl.Rows)
            {
                string val = row[1].ToString();
                string key = genKey(val);
                m_colls.Add(val);
                m_colls.Add(key);
                try
                {
                    m_maps.Add(key, val);
                }
                catch { }
            }
        }
        public void LoadData()
        {
            m_data.Load();
            Refresh();
        }
        public BindingSource m_bindingSrc
        {
            get { return m_data.m_bindingSource; }
        }
        public DataTable m_dataSource
        {
            get { return m_data.m_dataTable; }
        }
        public void Update(string selectedValue)
        {
            Debug.WriteLine("{0}.Update {1}", this, selectedValue);
            string key = genKey(selectedValue);
            if (!m_maps.ContainsKey(key))
            {
                m_colls.Add(key);
                m_colls.Add(selectedValue);
                m_maps.Add(key, selectedValue);
                //update database
                Add(selectedValue);
            }
        }
        private void Add(string newValue)
        {
            //single col tables
            DataRow newRow = m_dataSource.NewRow();
            newRow[1] = newValue;
            m_dataSource.Rows.Add(newRow);
            m_data.Submit();
        }
        public string find(string key)
        {
            key = genKey(key);
            if (m_maps.ContainsKey(key))
                return m_maps[key];
            else
                return null;
        }
    }
}