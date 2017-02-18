﻿#define use_progress

using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace test_binding
{
    [DataContract(Name = "Report")]
    public class lBaseReport : IDisposable, myCursor
    {
        [DataMember(Name = "rcName")]
        public string m_rcName;     //data set
        [DataMember(Name = "viewName")]
        public string m_viewName;   //data view
        [DataMember(Name = "rdlcPath")]
        public string m_rdlcPath;   //report template
        [DataMember(Name = "m_dsName")]
        public string m_dsName;     //data set name
#if crt_xml
        public string m_xmlPath;    //xml path
#endif
        public string m_pdfPath;    //print to pdf file
        public Dictionary<string, string> m_sqls;
        public static string s_dateFormat = "yyyy-MM-dd";

        protected lBaseReport()
        {
        }

        static public lBaseReport crtReport(lBaseReport m_report)
        {
            lBaseReport newRpt = new lBaseReport();
            newRpt.m_rcName = m_report.m_rcName;
            newRpt.m_viewName = m_report.m_viewName;
            newRpt.m_rdlcPath = m_report.m_rdlcPath;
            newRpt.m_dsName = m_report.m_dsName;
            newRpt.m_pdfPath = "report.pdf";
            return newRpt;
        }

        protected DataTable loadData(string qry)
        {
            DataTable dt = appConfig.s_contentProvider.GetData(qry);
            return dt;
        }

        protected DataTable loadData()
        {
            string qry = string.Format("SELECT * FROM {0}", m_viewName);
            DataTable dt = appConfig.s_contentProvider.GetData(qry);
            return dt;
        }

        private List<Stream> m_streams;
        private Stream CreateStream(string name,
          string fileNameExtension, Encoding encoding,
          string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }

        private void Export(LocalReport report)
        {
            string deviceInfo =
              @"<DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <PageWidth>8.5in</PageWidth>
                    <PageHeight>11in</PageHeight>
                    <MarginTop>0.25in</MarginTop>
                    <MarginLeft>0.25in</MarginLeft>
                    <MarginRight>0.25in</MarginRight>
                    <MarginBottom>0.25in</MarginBottom>
                </DeviceInfo>";
            Warning[] warnings;
            m_streams = new List<Stream>();
            report.Render("Image", deviceInfo, CreateStream, out warnings);
            foreach (Stream stream in m_streams)
            {
                stream.Position = 0;
            }
        }

        int m_currentPageIndex;
        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            Metafile pageImage = new Metafile(m_streams[m_currentPageIndex]);

            // Adjust rectangular area with printer margins.
            Rectangle adjustedRect = new Rectangle(
                ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                ev.PageBounds.Width,
                ev.PageBounds.Height);

            // Draw a white background for the report
            ev.Graphics.FillRectangle(Brushes.White, adjustedRect);

            // Draw the report content
            ev.Graphics.DrawImage(pageImage, adjustedRect);

            // Prepare for the next page. Make sure we haven't hit the end.
            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
        }

        private void Print()
        {
            if (m_streams == null || m_streams.Count == 0)
                throw new Exception("Error: no stream to print.");
            PrintDocument printDoc = new PrintDocument();
            if (!printDoc.PrinterSettings.IsValid)
            {
                throw new Exception("Error: cannot find the default printer.");
            }
            else
            {
                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                m_currentPageIndex = 0;
                printDoc.Print();
            }
        }

        public virtual List<ReportParameter> getReportParam()
        {
            List<ReportParameter> rpParams = new List<ReportParameter>();
            ReportParameter rpParam = new ReportParameter();
            rpParams.Add(rpParam);
            rpParam.Name = "details";

            string qry = string.Format("select DISTINCT[year] from {0} order by [year] desc", m_viewName);
            DataTable dt = appConfig.s_contentProvider.GetData(qry);
            for (int i = 0; i < 5; i++)
            {
                string val = i < dt.Rows.Count ? dt.Rows[i][0].ToString() : "0";
                Debug.WriteLine(string.Format("details({0}) {1}", i, val));
                rpParam.Values.Add(val);
            }

            // Set the report parameters for the report
            return rpParams;
        }

        private delegate void voidCaller();

        long m_iWork;
        public long getPos()
        {
            return m_iWork;
        }

        public void Run2()
        {
            ProgressDlg prg = new ProgressDlg();
            var d = new voidCaller(() => {
                //display wait msg
                prg.m_descr = "Loading data ...";

                LocalReport report = new LocalReport();

                //long time work
                DataSet ds = new DataSet();
                int step = 50 / m_sqls.Count;
                foreach (var pair in m_sqls)
                {
                    DataTable dt = loadData(pair.Value);
                    //after load data complete
                    m_iWork += step;

                    dt.TableName = pair.Key;
                    ds.Tables.Add(dt);

                    report.ReportPath = m_rdlcPath;
                    report.DataSources.Add(new ReportDataSource(pair.Key, dt));
                }

                //add report params
                List<ReportParameter> rpParams = getReportParam();
                report.SetParameters(rpParams);

                report.Refresh();

                //display wait msg
                prg.m_descr = "Exporting ...";

                //long time work
                Export(report);
                m_iWork = 100;

                ds.Clear();
                ds.Dispose();
                report.Dispose();
            });

            var t = d.BeginInvoke(null, null);
            m_iWork = 0;
            prg.m_endPos = 100;
            prg.m_cursor = this;
            prg.m_param = t;
            prg.ShowDialog();
            prg.Dispose();

            //print
#if false
            byte[] bytes = report.Render("PDF");
            FileStream fs = new FileStream(m_pdfPath, FileMode.Create);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
#else
            Print();
#endif
        }
        public virtual void Run()
        {
            ProgressDlg prg = new ProgressDlg();
            var d = new voidCaller(()=> {

                //display wait msg
                prg.m_descr = "Load view data ...";

                //long time work
                DataTable dt = loadData();
                m_iWork = 50;

                //after load data complete
                dt.TableName = m_viewName;
#if crt_xml
                dt.WriteXml(m_xmlPath);
#endif
                LocalReport report = new LocalReport();
                report.ReportPath = m_rdlcPath;
                report.DataSources.Add(new ReportDataSource(m_rcName, dt));

                //add report params
                List<ReportParameter> rpParams = getReportParam();
                report.SetParameters(rpParams);

                report.Refresh();

                //display wait msg
                prg.m_descr = "Exporting ...";

                //long time work
                Export(report);
                m_iWork = 100;

                dt.Dispose();
                report.Dispose();
            });

            var t = d.BeginInvoke(null, null);
            m_iWork = 0;
            prg.m_endPos = 100;
            prg.m_cursor = this;
            prg.m_param = t;
            prg.ShowDialog();
            prg.Dispose();

            //print
#if false
            byte[] bytes = report.Render("PDF");
            FileStream fs = new FileStream(m_pdfPath, FileMode.Create);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
#else
            Print();
#endif
        }
        #region dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~lBaseReport()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_streams != null)
                {
                    foreach (Stream stream in m_streams)
                        stream.Close();
                    m_streams = null;
                }
            }
        }
        #endregion
    }

    [DataContract(Name = "ReceiptsReport")]
    public class lReceiptsReport : lBaseReport
    {
        public lReceiptsReport()
        {
            m_rcName = "DataSet1";
            m_viewName = "v_receipts";
            m_rdlcPath = @"..\..\rpt_receipts.rdlc";
            m_dsName = "DataSet1";
            m_pdfPath = @"..\..\report.pdf";
        }
    }
    [DataContract(Name = "InternalPaymentReport")]
    public class lInternalPaymentReport : lBaseReport
    {
        public lInternalPaymentReport()
        {
            m_rcName = "DataSet1";
            m_viewName = "v_internal_payment";
            m_rdlcPath = @"..\..\rpt_interpayment.rdlc";
            m_dsName = "DataSet1";
            m_pdfPath = @"..\..\report.pdf";
        }
    }
    [DataContract(Name = "ExternalPaymentReport")]
    public class lExternalPaymentReport : lBaseReport
    {
        public lExternalPaymentReport()
        {
            m_rcName = "DataSet1";
            m_viewName = "v_external_payment";
            m_rdlcPath = @"..\..\rpt_exterpayment.rdlc";
            m_dsName = "DataSet1";
            m_pdfPath = @"..\..\report.pdf";
        }
    }
    [DataContract(Name = "SalaryReport")]
    public class lSalaryReport : lBaseReport
    {
        public lSalaryReport()
        {
            m_rcName = "DataSet1";
            m_viewName = "v_salary";
            m_rdlcPath = @"..\..\rpt_salary.rdlc";
            m_dsName = "DataSet1";
            m_pdfPath = @"..\..\report.pdf";
        }
    }


    public class lDaysReport : lBaseReport
    {
        List<ReportParameter> m_rptParams;
        protected virtual string getDateQry(string zStartDate, string zEndDate)
        {
            string qryDaysData = string.Format("select group_name, date, name,"
                + " sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary"
                + " from"
                + " (select group_name, date, name, actually_spent as inter_pay, 0 as exter_pay, 0 as salary"
                + " from internal_payment where date between '{0} 00:00:00' and '{1} 00:00:00'"
                + " union"
                + " select group_name, date, name, 0 as inter_pay, spent as exter_pay, 0 as salary"
                + " from external_payment where date between '{0} 00:00:00' and '{1} 00:00:00'"
                + " union"
                + " select group_name, date, name, 0 as inter_pay, 0 as exter_pay, salary"
                + " from salary where date between '{0} 00:00:00' and '{1} 00:00:00')"
                + " group by group_name, date, name",
                zStartDate, zEndDate);
            return qryDaysData;
        }

        protected string getMonthQry(string zStartDate, string zEndDate)
        {
            string qryMonthData = string.Format("select month, sum(receipt) as receipt, sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary, 0 as remain "
                + " from("
                + "   select strftime('%Y-%m', date) as month, 0 as receipt, sum(actually_spent) as inter_pay, 0 as exter_pay, 0 as salary"
                + "   from internal_payment where date between '{0} 00:00:00' and '{1} 00:00:00' group by month"
                + "   union"
                + "   select strftime('%Y-%m', date) as month, 0 as receipt, 0 as inter_pay, sum(spent) as exter_pay, 0 as salary"
                + "   from external_payment where date between '{0} 00:00:00' and '{1} 00:00:00' group by month"
                + "   union"
                + "   select strftime('%Y-%m', date) as month, 0 as receipt, 0 as inter_pay, 0 as exter_pay, sum(salary) as salary"
                + "   from salary where date between '{0} 00:00:00' and '{1} 00:00:00' group by month"
                + "   union"
                + "   select strftime('%Y-%m', date) as month, sum(amount) as receipt, 0 as inter_pay, 0 as exter_pay, 0 as salary"
                + "   from receipts where date between '{0} 00:00:00' and '{1} 00:00:00' group by month"
                + " ) group by month",
                zStartDate, zEndDate);
            return qryMonthData;
        }
        protected virtual string getType()
        {
            return "Ngày";
        }
        public lDaysReport(DateTime startDate, DateTime endDate)
        {
            string zStartDate = startDate.ToString(s_dateFormat);
            string zEndDate = endDate.ToString(s_dateFormat);
            m_rptParams = new List<ReportParameter>()
            {
                new ReportParameter("startDate",zStartDate),
                new ReportParameter("endDate",zEndDate),
                new ReportParameter( "type", getType())
            };

            m_sqls = new Dictionary<string, string>
            {
                { "DataSet1", getDateQry(zStartDate, zEndDate)},
                { "DataSet2", getMonthQry(zStartDate, zEndDate)}
            };

            m_rdlcPath = @"..\..\rpt_days.rdlc";
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_sqls.Clear();
            }
            base.Dispose(disposing);
        }

        public override List<ReportParameter> getReportParam()
        {
            return m_rptParams;
        }
    }
    public class lWeekReport : lDaysReport
    {
        protected override string getDateQry(string zStartDate, string zEndDate)
        {
            string qryWeeksData = string.Format("select group_name, week as date, '' as name,"
               + " sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary"
               + " from"
               + " (select group_name, strftime('%Y-%W', date) as week, actually_spent as inter_pay, 0 as exter_pay, 0 as salary"
               + " from internal_payment where date between '{0} 00:00:00' and '{1} 00:00:00'"
               + " union"
               + " select group_name, strftime('%Y-%W', date) as week, 0 as inter_pay, spent as exter_pay, 0 as salary"
               + " from external_payment where date between '{0} 00:00:00' and '{1} 00:00:00'"
               + " union"
               + " select group_name, strftime('%Y-%W', date) as week, 0 as inter_pay, 0 as exter_pay, salary"
               + " from salary where date between '{0} 00:00:00' and '{1} 00:00:00')"
               + " group by group_name, week",
               zStartDate, zEndDate);
            return qryWeeksData;
        }
        protected override string getType()
        {
            return "Tuần";
        }
        public lWeekReport(DateTime startDate, DateTime endDate) : base(endDate, startDate)
        {
        }
    }
    public class lMonthReport : lDaysReport
    {
        protected override string getDateQry(string zStartDate, string zEndDate)
        {
            string qryMonthsData = string.Format("select group_name, month as date, '' as name,"
               + " sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary"
               + " from"
               + " (select group_name, strftime('%Y-%m', date) as month, actually_spent as inter_pay, 0 as exter_pay, 0 as salary"
               + " from internal_payment where date between '{0} 00:00:00' and '{1} 00:00:00'"
               + " union"
               + " select group_name, strftime('%Y-%m', date) as month, 0 as inter_pay, spent as exter_pay, 0 as salary"
               + " from external_payment where date between '{0} 00:00:00' and '{1} 00:00:00'"
               + " union"
               + " select group_name, strftime('%Y-%m', date) as month, 0 as inter_pay, 0 as exter_pay, salary"
               + " from salary where date between '{0} 00:00:00' and '{1} 00:00:00')"
               + " group by group_name, month",
               zStartDate, zEndDate);
            return qryMonthsData;
        }
        protected override string getType()
        {
            return "Tháng";
        }
        public lMonthReport(DateTime startDate, DateTime endDate) : base(endDate, startDate)
        {
        }
    }

    public class lBuildingReport : lBaseReport
    {
        public string m_buildingName;
        public DateTime m_startDate;
        public DateTime m_endDate;
        List<ReportParameter> m_rptParams;
        public lBuildingReport(string building, DateTime startDate, DateTime endDate)
        {
            string zStartDate = startDate.ToString(s_dateFormat);
            string zEndDate = endDate.ToString(s_dateFormat);
            m_buildingName = building;
            m_rptParams = new List<ReportParameter>()
            {
                new ReportParameter("startDate",zStartDate),
                new ReportParameter("endDate",zEndDate),
                new ReportParameter("buildingName", m_buildingName)
            };
            string qry = string.Format("select * from external_payment"
                + " where building like '%{0}%' and date between '{1} 00:00:00' and '{2} 00:00:00'"
                + " order by date",
                building, zStartDate, zEndDate);
            m_sqls = new Dictionary<string, string>
            {
                { "DataSet1", qry }
            };
            m_rdlcPath = @"..\..\rpt_building.rdlc";
        }
        public override List<ReportParameter> getReportParam()
        {
            return m_rptParams;
        }
    }
}