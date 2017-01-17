﻿using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace test_binding
{

    public partial class Form1 : Form
    {

        class lBaseReport : IDisposable
        {
            public string m_rcName;     //data set
            public string m_viewName;   //data view
            public string m_rdlcPath;   //report template
#if crt_xml
        public string m_xmlPath;    //xml path
#endif
            public string m_pdfPath;    //print to pdf file
            public string m_dsName;     //data set name

            protected lContentProvider m_cp;

            public lBaseReport(lContentProvider cp)
            {
                m_cp = cp;
            }
            private DataTable loadData()
            {
                string qry = string.Format("SELECT * FROM {0}", m_viewName);
                DataTable dt = m_cp.getData(qry);
                dt.TableName = m_viewName;
#if crt_xml
                dt.WriteXml(m_xmlPath);
#endif
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
                DataTable dt = m_cp.getData(qry);
                for (int i = 0; i< 5;i++)
                {
                    string val = "0";
                    if (i < dt.Rows.Count)
                    {
                        val = dt.Rows[i][0].ToString();
                    }
                    Debug.WriteLine(string.Format("details({0}) {1}", i, val));
                    rpParam.Values.Add("0");
                }

                // Set the report parameters for the report
                return rpParams;
            }

            public void Run()
            {
                LocalReport report = new LocalReport();
                report.ReportPath = m_rdlcPath;
                DataTable dt = loadData();
                report.DataSources.Add(new ReportDataSource(m_rcName, dt));

                //add report params
                List<ReportParameter> rpParams = getReportParam();
                report.SetParameters(rpParams);

                report.Refresh();
#if false
                byte[] bytes = report.Render("PDF");
                FileStream fs = new FileStream(m_pdfPath, FileMode.Create);
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
#else
                Export(report);
                Print();
#endif
            }
            public void Dispose()
            {
                if (m_streams != null)
                {
                    foreach (Stream stream in m_streams)
                        stream.Close();
                    m_streams = null;
                }
            }
        }

        class lReceiptsReport : lBaseReport
        {
            public lReceiptsReport(lContentProvider cp):base(cp)
            {
                m_rcName = "DataSet1";
                m_viewName = "v_receipts";
                m_rdlcPath = @"..\..\receipts.rdlc";
                m_dsName = "DataSet1";
                m_pdfPath = @"..\..\report.pdf";
            }
            public override List<ReportParameter> getReportParam()
            {
                return new List<ReportParameter>();
            }
        }
        class lInternalPaymentReport : lBaseReport
        {
            public lInternalPaymentReport(lContentProvider cp):base(cp)
            {
                m_rcName = "DataSet1";
                m_viewName = "v_internal_payment";
                m_rdlcPath = @"..\..\internal_payment.rdlc";
                m_dsName = "DataSet1";
                m_pdfPath = @"..\..\report.pdf";
            }
        }
        class lExternalPaymentReport : lBaseReport
        {
            public lExternalPaymentReport(lContentProvider cp) :base(cp)
            {
                m_rcName = "DataSet1";
                m_viewName = "v_external_payment";
                m_rdlcPath = @"..\..\external_payment.rdlc";
                m_dsName = "DataSet1";
                m_pdfPath = @"..\..\report.pdf";
            }
        }
        class lSalaryReport : lBaseReport
        {
            public lSalaryReport(lContentProvider cp):base(cp)
            {
                m_rcName = "DataSet1";
                m_viewName = "v_salary";
                m_rdlcPath = @"..\..\Report1.rdlc";
                m_dsName = "DataSet1";
                m_pdfPath = @"..\..\report.pdf";
            }
        }
    }
}