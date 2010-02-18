using System;
using System.Xml;
using System.Xml.Xsl;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Plexus.Utils
{
    /// <summary>
    /// Any data/value manipulate class.
    /// </summary>
    public class DataManipulation
    {
        private System.Web.HttpResponse response;
        private DataManipulation.ApplicationType appType;

        /// <summary>
        /// Indicate application type (Windows, or Web application).
        /// </summary>
        public enum ApplicationType
        {
            /// <summary>
            /// Windows Application.
            /// </summary>
            Win,
            /// <summary>
            /// Web application.
            /// </summary>
            Web
        }

        /// <summary>
        /// Indicate data type of output.
        /// </summary>
        public enum DataType
        {
            /// <summary>
            /// Comma Delimited (will lost the format and error for those data has 'comma').
            /// </summary>
            CSV,

            /// <summary>
            /// Excel file.
            /// </summary>
            Excel,

            /// <summary>
            /// Portable Data File.
            /// </summary>
            PDF,

            /// <summary>
            /// XML.
            /// </summary>
            XML
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataManipulation()
        {
        }

        /// <summary>
        /// Constructor with application type given (recommended).
        /// </summary>
        public DataManipulation(DataManipulation.ApplicationType _appType)
        {
            appType = _appType;
            if (appType == DataManipulation.ApplicationType.Web)
                response = System.Web.HttpContext.Current.Response;
        }

        /// <summary>
        /// Export to desired file type.
        /// </summary>
        /// <param name="formatType">Excel,CSV,PDF,XML</param>
        /// <param name="detailsTable">Source table</param>
        /// <param name="headers">Desired label for header</param>
        /// <param name="fileName">File name only (exclude location)</param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// DataManipulation lcls_data = new DataManipulation(DataManipulation.ApplicationType.Web);
        /// string fileName = "TestCSV2.csv";
        /// lcls_data.Export(DataManipulation.DataType.CSV,(DataTable)DataGrid1.DataSource,fileName);
        /// </code>
        /// </example>
        public bool Export(DataManipulation.DataType formatType, DataTable detailsTable, string[] headers, string fileName)
        {
            try
            {
                if (detailsTable == null)
                {
                    Logger.Error(typeof(DataManipulation), "Blank table");
                    throw new Exception("Blank table");
                }
                if (detailsTable.Rows.Count == 0)
                {
                    Logger.Error(typeof(DataManipulation), "Nothing to export");
                    throw new Exception("Nothing to export");
                }
                if (fileName == null)
                {
                    Random ran = new Random();
                    fileName = ran.Next().ToString();
                }

                // Create Dataset
                DataSet dsExport = new DataSet("Export");
                DataTable dtExport = new DataTable();
                #region convert datetime column -> string column
                //sotat excel able to display as datetime value
                for (int i = 0; i < detailsTable.Columns.Count; i++)
                {
                    DataColumn dc = new DataColumn();
                    if (detailsTable.Columns[i].DataType == typeof(DateTime))
                        dc = new DataColumn(detailsTable.Columns[i].ColumnName, typeof(string));
                    else
                        dc = new DataColumn(detailsTable.Columns[i].ColumnName, detailsTable.Columns[i].DataType);
                    dtExport.Columns.Add(dc);
                }//end loops

                for (int i = 0; i < detailsTable.Rows.Count; i++)
                {
                    DataRow newRow = dtExport.NewRow();
                    for (int j = 0; j < detailsTable.Columns.Count; j++)
                    {
                        if (detailsTable.Columns[j].DataType == typeof(DateTime))
                        {
                            if (detailsTable.Rows[i][j] == DBNull.Value)
                                newRow[j] = "";
                            else
                            {
                                //skip date min
                                DateTime date = Convert.ToDateTime(detailsTable.Rows[i][j]);
                                if (date.Year > 1900)
                                    newRow[j] = date.ToString("M/d/yyyy hh:mm tt");
                            }
                        }
                        else
                            newRow[j] = detailsTable.Rows[i][j];
                    }//end loops

                    dtExport.Rows.Add(newRow);
                }//end loops
                dtExport.AcceptChanges();
                #endregion
                dtExport.TableName = "Values";
                dsExport.Tables.Add(dtExport);

                // Getting Field Names
                string[] sHeaders = new string[dtExport.Columns.Count];
                string[] sFileds = new string[dtExport.Columns.Count];
                for (int i = 0; i < dtExport.Columns.Count; i++)
                {
                    sHeaders[i] = dtExport.Columns[i].ColumnName;
                    sFileds[i] = dtExport.Columns[i].ColumnName;
                }

                if (headers != null && headers.Length == dtExport.Columns.Count)
                    sHeaders = headers;

                if (formatType == DataType.PDF)
                    ExportPdfWeb(detailsTable, fileName, headers);
                else
                {
                    if (appType == ApplicationType.Web)
                        ExportXSLTWeb(formatType, dsExport, sHeaders, sFileds, fileName);
                    else if (appType == ApplicationType.Win)
                        ExportXSLTWindows(formatType, dsExport, sHeaders, sFileds, fileName);
                }

                return true;
            }
            catch (ThreadAbortException ex)
            {
                //this is not an appliction error but throw thread
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DataManipulation), ex);
                throw ex;
            }
        }

        #region Export to CSV, Excel or PDF
        /// <summary>
        /// Export 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        /// <see>http://itextsharp.sourceforge.net/tutorial/ch05.html</see>
        /// <see>http://www.nabble.com/memory-stream-in-PdfWriter-td4410917.html</see>
        /// <see>http://support.microsoft.com/kb/306654</see>
        public void ExportPdfWeb(DataTable table, string fileName, string[] headers)
        {
            try
            {
                // Appending Headers
                response.Clear();
                response.Buffer = true;
                response.ContentType = "application/pdf";
                response.AppendHeader("content-disposition", "attachment; filename=" + fileName);

                //MemoryStream stream = new MemoryStream();
                FileStream stream = new FileStream(fileName, FileMode.Create);
                Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();
                LoadDocument(document, table, null);
                document.Close();

                //PdfAcroForm form = writer.AcroForm;
                //document.Open();
                //PdfReader reader = new PdfReader(fileName);
                //PdfStream pdfStream = new PdfStream(stream, writer);
                //response.Write(pdfStream.ToString());
                //PdfStamper stamper = new PdfStamper(reader, memory);

                //Writeout the Content
                response.WriteFile(fileName);
                //response.BinaryWrite(stream.ToArray());
                response.End();
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DataManipulation), ex);
                throw ex;
            }
        }
        private void LoadDocument(Document document, DataTable source, float[] headerWidths)
        {
            try
            {
                // we add some meta information to the document
                PdfPTable datatable = new PdfPTable(source.Columns.Count);

                datatable.DefaultCell.Padding = 1;
                if (headerWidths != null && headerWidths.Length > 0) datatable.SetWidths(headerWidths);
                datatable.WidthPercentage = 100; // percentage

                datatable.DefaultCell.BorderWidth = .4f;
                datatable.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                foreach (DataColumn column in source.Columns)
                    datatable.AddCell(column.ColumnName);
                datatable.HeaderRows = 1;  // this is the end of the table header

                datatable.DefaultCell.BorderWidth = .4f;
                for (int i = 0; i < source.Rows.Count; i++)
                {
                    if (i % 2 == 1) datatable.DefaultCell.GrayFill = 0.9f;
                    for (int x = 0; x < source.Columns.Count; x++)
                        datatable.AddCell(source.Rows[i][x].ToString());
                    if (i % 2 == 1) datatable.DefaultCell.GrayFill = 1.0f;
                }
                document.Add(datatable);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        private void ExportXSLTWeb(DataManipulation.DataType formatType, DataSet dataSet, string[] headers, string[] fileds, string fileName)
        {
            try
            {
                // Appending Headers
                response.Clear();
                response.Buffer = true;
                if (formatType == DataType.CSV)
                    response.ContentType = "text/csv";
                else if (formatType == DataType.Excel)
                    response.ContentType = "application/vnd.ms-excel";
                response.AppendHeader("content-disposition", "attachment; filename=" + fileName);
                response.Charset = "utf-16";

                // XSLT to use for transforming this dataset.						
                MemoryStream stream = new MemoryStream();
                //FileStream stream = new FileStream(fileName, FileMode.Create);
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Unicode);

                CreateStylesheet(writer, formatType, headers, fileds);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                XmlDataDocument xmlDoc = new XmlDataDocument(dataSet);
                XslTransform xslTran = new XslTransform();
                xslTran.Load(new XmlTextReader(stream), null, null);
                //HandleEscapeCharacter(ref xmlDoc);

                System.IO.StringWriter sw = new System.IO.StringWriter();
                xslTran.Transform(xmlDoc, null, sw, null);

                //Writeout the Content
                response.Write(sw.ToString());
                //response.WriteFile(fileName);
                sw.Close();
                writer.Close();
                stream.Close();
                response.End();
            }
            catch (ThreadAbortException ex)
            {
                //this is not an appliction error but throw thread
                return;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DataManipulation), ex);
                throw ex;
            }
        }
        private void ExportXSLTWindows(DataManipulation.DataType FormatType, DataSet dsExport, string[] sHeaders, string[] sFileds, string FileName)
        {
            MemoryStream stream = null;
            XmlTextWriter writer = null;
            StreamWriter strwriter = null;
            StringWriter sw = null;

            try
            {
                // XSLT to use for transforming this dataset
                stream = new MemoryStream();
                writer = new XmlTextWriter(stream, Encoding.UTF8);

                CreateStylesheet(writer, FormatType, sHeaders, sFileds);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                sw = new System.IO.StringWriter();
                XmlDataDocument xmlDoc = new XmlDataDocument(dsExport);
                XslTransform xslTran = new XslTransform();
                xslTran.Load(new XmlTextReader(stream), null, null);
                xslTran.Transform(xmlDoc, null, sw, null);

                //Writeout the Content									
                strwriter = new StreamWriter(FileName);
                strwriter.WriteLine(sw.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                strwriter.Close();
                sw.Close();
                writer.Close();
                stream.Close();
            }
        }
        private void CreateStylesheet(XmlTextWriter writer, DataManipulation.DataType FormatType, string[] sHeaders, string[] sFileds)
        {
            try
            {
                // xsl:stylesheet
                string ns = "http://www.w3.org/1999/XSL/Transform";
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("xsl", "stylesheet", ns);
                writer.WriteAttributeString("version", "1.0");
                writer.WriteStartElement("xsl:output");
                writer.WriteAttributeString("method", "text");
                writer.WriteAttributeString("version", "4.0");
                writer.WriteAttributeString("encoding", "utf-16");//support unicode
                writer.WriteEndElement();

                // xsl-template
                writer.WriteStartElement("xsl:template");
                writer.WriteAttributeString("match", "/");

                // xsl:value-of for headers
                for (int i = 0; i < sHeaders.Length; i++)
                {
                    writer.WriteString("\"");
                    writer.WriteStartElement("xsl:value-of");
                    writer.WriteAttributeString("select", "'" + sHeaders[i] + "'");
                    writer.WriteEndElement(); // xsl:value-of
                    writer.WriteString("\"");
                    if (i != sFileds.Length - 1) writer.WriteString((FormatType == DataType.CSV) ? "," : "	");
                }

                // xsl:for-each
                writer.WriteStartElement("xsl:for-each");
                writer.WriteAttributeString("select", "Export/Values");
                writer.WriteString("\r\n");

                // xsl:value-of for data fields
                for (int i = 0; i < sFileds.Length; i++)
                {
                    writer.WriteString("\"");
                    writer.WriteStartElement("xsl:value-of");
                    writer.WriteAttributeString("select", sFileds[i]);
                    writer.WriteEndElement(); // xsl:value-of
                    writer.WriteString("\"");
                    if (i != sFileds.Length - 1) writer.WriteString((FormatType == DataType.CSV) ? "," : "	");
                }

                writer.WriteEndElement(); // xsl:for-each
                writer.WriteEndElement(); // xsl-template
                writer.WriteEndElement(); // xsl:stylesheet
                writer.WriteEndDocument();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                //logger.Error(ex);
                throw ex;
            }
        }
        #endregion

        #region Data Utilities
        /// <summary>
        /// Parse or convert date time value to string.
        /// </summary>
        /// <param name="val">DateTime value.</param>
        /// <param name="format">DateTime format.</param>
        /// <returns></returns>
        public static string ParseToString(DateTime val, string format)
        {
            //after add timezone calculation //060412tys han chew b'day
            if (val <= new DateTime(1900, 1, 2) || val <= DateTime.MinValue)	//.Subtract(new TimeSpan(1,0,0,0) ) )
                return "";
            else
                return val.ToString(format);
        }
        /// <summary>
        /// Convert string to integer (handle empty string).
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int ConvertToInteger(string val)
        {
            if (val == null || val == string.Empty)
                return 0;

            if (Validator.IsInteger(val))
                return Convert.ToInt32(val);

            return 0;
        }

        /// <summary>
        /// Convert to double value.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static double ConvertToDouble(string sender)
        {
            if (sender == null) return 0;
            if (sender == string.Empty) return 0;
            return Convert.ToDouble(sender);
        }
        /// <summary>
        /// Make sure is a valid long value.
        /// </summary>
        /// <param name="val">String to check.</param>
        /// <returns></returns>
        private static long ConvertToLong(string val)
        {
            if (Validator.IsInteger(val))
                return Convert.ToInt64(val);
            else
                return 00000000;
        }
        /// <summary>
        /// Universal converting.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static long ConvertToLong(object sender)
        {
            if (sender == null) return 00000000;

            if (sender.GetType() == typeof(string))
                return ConvertToLong(sender.ToString());
            else
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Convert decimal value to time span.
        /// </summary>
        /// <param name="val">Decimal value.</param>
        /// <returns>Within a day interval only.</returns>
        public static TimeSpan ConvertToTimeSpan(decimal val)
        {
            TimeSpan diff = new TimeSpan(0, 0, 0);

            int hours = (int)Math.Floor((double)val);
            int minutes = (int)(val * 60) % 60;
            int seconds = (int)Math.Floor((double)(val * 60 * 60) % 60);//060404tys advice by seow hua
            diff = new TimeSpan(hours, minutes, seconds);
            return diff;
        }
        /// <summary>
        /// Split time zone value into hour and minute.
        /// </summary>
        /// <param name="sender">Time zone value.</param>
        /// <param name="hour">Return integer hour.</param>
        /// <param name="minute">Return integer minute.</param>
        public static void SplitTimeZone(double sender, out int hour, out int minute)
        {
            hour = 0;
            minute = 0;
            hour = (int)Math.Floor(sender);
            minute = (int)(sender - hour) * 60;
        }
        /// <summary>
        /// Convert ID to desired ticketing format.
        /// </summary>
        /// <param name="prefix">eg. ERR0001, ESC0346346.</param>
        /// <param name="id">Only ID number.</param>
        /// <param name="length">Total length to show.</param>
        /// <returns></returns>
        public static string ConvertToFormat(string prefix, int id, int length)
        {
            return prefix + id.ToString().PadLeft(length - prefix.Length, '0');
        }

        /// <summary>
        /// Convert to recognized date time value.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(object sender)
        {
            if (sender == null) return new DateTime(1900, 1, 1);

            if (sender.GetType() == typeof(long))
            {
                //DateTime output = new DateTime(1900, 1, 1);
                //output = output.AddTicks(Convert.ToInt64(sender));
                //return output;
            }
            else if (sender.GetType() == typeof(Int64))
            {
                //DateTime output = new DateTime(1900, 1, 1);
                //output = output.AddTicks(Convert.ToInt64(sender));
                //return output;
            }
            else if (sender.GetType() == typeof(Decimal))
            {
                DateTime output = new DateTime(1700, 1, 1);
                output = output.AddSeconds(Convert.ToDouble(sender));
                output = output.ToLocalTime();
                return output;
            }
            else if (sender.GetType() == typeof(string))
            {
                if (Validator.IsDateTime(sender.ToString()))
                    return Convert.ToDateTime(sender);
            }

            return new DateTime(1900, 1, 1);
        }
        /// <summary>
        /// Get a new numbering.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static string GetNewNumbering(string source, int increment)
        {
            string output = "";

            try
            {
                int indexStart = -1;
                int indexEnd = source.Length - 1;
                for (int i = 0; i < source.Length; i++)
                {
                    string s = source.Substring(i, 1);
                    if (indexStart == -1)
                    {
                        if (Validator.IsInteger(s))
                            indexStart = i;
                    }
                    else
                    {
                        if (!Validator.IsInteger(s))
                        {
                            indexEnd = i - 1;
                            break;
                        }
                    }
                }//end loops


                string ls_Check = source.Substring(indexStart, indexEnd - indexStart + 1);
                int newNumber = 0;
                newNumber = Convert.ToInt32(ls_Check);
                newNumber += increment;


                output += source.Substring(0, indexStart);
                output += newNumber.ToString().PadLeft(indexEnd - indexStart + 1, '0');
                output += source.Substring(indexEnd + 1, source.Length - 1 - indexEnd);
                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ////logger.Error(ex);
                return output;
            }
        }

        /// <summary>
        /// Get file name part only for a full path.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFileNameOnly(string source)
        {
            string output = source;
            int i = output.LastIndexOf("/");
            if (i > -1)
                output = output.Substring(i + 1, output.Length - i - 1);
            else
            {
                i = output.LastIndexOf("\\");
                if (i > -1)
                    output = output.Substring(i + 1, output.Length - i - 1);
            }


            return output;
        }
        /// <summary>
        /// Get file path/directory only (ignore file name).
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFilePathOnly(string source)
        {
            string output = source;
            int i = output.LastIndexOf("/");
            if (i > -1)
                output = output.Substring(0, i);
            else
            {
                i = output.LastIndexOf("\\");
                if (i > -1)
                    output = output.Substring(0, i);
            }


            return output;
        }
        public static string GetRootPath(string fullPath, string pageOnly)
        {
            string output = fullPath.Replace(pageOnly, string.Empty);

            return output;
        }

        /// <summary>
        /// Convert joined text to database understanded string.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string ConvertToSqlINValue(string source, char seperator)
        {
            string output = "";
            string[] sources = source.Split(seperator);
            foreach (string s in sources)
                output += "'" + s + "',";

            return output.Trim(',');
        }
        /// <summary>
        /// Split data from database to class usesable value type.
        /// </summary>
        /// <param name="join"></param>
        /// <param name="seperator">Seperator charactor.</param>
        /// <returns></returns>
        public static string[] Split(string join, char seperator)
        {
            join = join.Trim();

            string[] joins = join.Split(seperator);

            #region fine tune - get rid of blank value //060714tys
            ArrayList list = new ArrayList();
            foreach (string s in joins)
            {
                if (s != string.Empty)
                    list.Add(s.Trim());//060818tys
            }//end loops
            #endregion

            joins = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
                joins[i] = list[i].ToString();

            return joins;
        }
        /// <summary>
        /// Join string array with ';' during save into database.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="seperator">seperator character.</param>
        /// <returns></returns>
        public static string Join(string[] source, char seperator)
        {
            string output = "";
            if (source == null) return output;

            for (int i = 0; i < source.Length; i++)
                output += source[i] + seperator;
            output = output.TrimEnd(seperator);

            return output;
        }

        /// <summary>
        /// If it is xml reserved character then parsing a correct string otherwise ignore.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ParseXmlReservedCharacter(string sender)
        {
            string output = sender;
            //if(sender.IndexOf("\"")>-1)
            //	output = sender.Replace("\"","&quot;");
            if (sender.IndexOf("'") > -1)
                output = sender.Replace("'", "&#39;");

            return output;
        }
        /// <summary>
        /// Reversing into corresponding or orginal string value.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ReverseXmlReservedCharacter(string sender)
        {
            string output = sender;
            //if(sender.IndexOf("&quot;")>-1)
            //	output = sender.Replace("&quot;","\"");
            if (sender.IndexOf("&#39;") > -1)
                output = sender.Replace("&#39;", "'");

            return output;
        }
        /// <summary>
        /// Make in become mix format string.
        /// </summary>
        /// <param name="sender">String to convert.</param>
        /// <returns>Mix case format.</returns>
        /// <example>
        /// TODO: "AGILENT TECHNOLOGIES" become "Agilent Technologies".
        /// </example>
        public static string MixCase(string sender)
        {
            string output = sender;

            try
            {
                char seperator = ' ';
                string[] outputs = sender.Split(new char[] { seperator });
                if (outputs.Length > 1) output = string.Empty;
                foreach (string s in outputs)
                    output += s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length - 1).ToLower() + seperator;
                output = output.TrimEnd(new char[] { seperator });

                Logger.Info(typeof(DataManipulation), output);
                Logger.Debug(typeof(DataManipulation), output);
                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }

        /// <summary>
        /// Get config key within section.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ArrayList ReadXml(string path, string section)
        {
            ArrayList list = new ArrayList();
            XmlDocument configXml = new XmlDocument();
            XmlTextReader configXmlReader = new XmlTextReader(path);

            try
            {
                configXml.Load(configXmlReader);
                XmlNodeList configNodes = configXml.GetElementsByTagName(section);
                foreach (XmlNode configNode in configNodes)
                {
                    if (configNode.LocalName == section)
                    {
                        foreach (XmlNode node in configNode)
                            list.Add(new ListItem(node.Attributes["key"].Value, node.Attributes["value"].Value));//.Name, node.Value));
                    }
                }//end loops

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DataManipulation), ex);
                return list;
            }
            finally { configXmlReader.Close(); }
        }
        /// <summary>
        /// Converts an IEnumerable type collection into a DataTable
        /// </summary>
        /// <param name="collection">Collection type that implements IEnumberable</param>
        /// <returns>Datatable representing IEnumerable collection</returns>
        /// <see>http://www.developerfusion.com/community/blog-entry/8390164/convert-linq-resultset-to-datatable/</see>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-04-13<br/>
        /// </remarks>
        public static DataTable ToDataTable<T>(IEnumerable<T> collection)
        {
            if (collection == null) return null;
            // Retrieve the Type passed into the Method
            Type _impliedType = typeof(T);

            // Create DataTable to Fill
            DataTable _newDataTable = new DataTable(_impliedType.Name);

            //Get an array of the Type's properties
            PropertyInfo[] _propInfo = _impliedType.GetProperties();

            //Create the columns in the DataTable
            foreach (PropertyInfo pi in _propInfo)
            {
                if (pi.PropertyType == typeof(System.Nullable<short>))
                    _newDataTable.Columns.Add(pi.Name, typeof(short));
                else if (pi.PropertyType == typeof(System.Nullable<decimal>))
                    _newDataTable.Columns.Add(pi.Name, typeof(decimal));
                else if (pi.PropertyType == typeof(System.Nullable<DateTime>))
                    _newDataTable.Columns.Add(pi.Name, typeof(DateTime));
                else
                    _newDataTable.Columns.Add(pi.Name, typeof(string));//hack: change to original datatype// pi.PropertyType);
            }

            //Populate the table
            foreach (T item in collection)
            {
                DataRow _newDataRow = _newDataTable.NewRow();
                _newDataRow.BeginEdit();
                foreach (PropertyInfo pi in _propInfo)
                    _newDataRow[pi.Name] = (pi.GetValue(item, null) == null) ?
                        DBNull.Value :
                        pi.GetValue(item, null);
                _newDataRow.EndEdit();
                _newDataTable.Rows.Add(_newDataRow);
            }

            return _newDataTable;
        }
        /// <summary>
        /// TODO: Converts an IEnumerable type collection into a DataTable
        /// </summary>
        /// <param name="collection">Collection type that implements IEnumberable</param>
        /// <returns>Datatable representing IEnumerable collection</returns>
        /// <see>http://www.developerfusion.com/community/blog-entry/8390164/convert-linq-resultset-to-datatable/</see>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-04-13<br/>
        /// </remarks>
        public static DataTable ToDataTable<T>(IEnumerable<T> collection, string targetCollection)
        {
            if (collection == null) return null;
            // Retrieve the Type passed into the Method
            Type _impliedType = typeof(T);

            // Create DataTable to Fill
            DataTable _newDataTable = new DataTable(_impliedType.Name);

            //Get an array of the Type's properties
            PropertyInfo[] _propInfo = _impliedType.GetProperties();

            //Create the columns in the DataTable
            foreach (PropertyInfo pi in _propInfo)
            {
                if (targetCollection != null && pi.Name == targetCollection)
                {
                    Type targetType = pi.PropertyType.GetGenericArguments()[0];
                    BindingFlags flags = BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                    //FieldInfo field = collection.GetType().GetField(targetCollection, flags);
                    object obj = pi.GetValue(collection, null);
                    IEnumerable<PropertyInfo> target = null;
                    target = (IEnumerable<PropertyInfo>)obj;
                    //target = (IEnumerable<PropertyInfo>)pi.GetValue(target, null);
                    //target = (IEnumerable<PropertyInfo>)collection.GetType().UnderlyingSystemType.InvokeMember(targetCollection, flags, null, target, null);
                    //targetType.UnderlyingSystemType.InvokeMember(targetCollection, flags, null, collection, null);
                    AddDataTableSchema(ref _newDataTable,target);
                }
                if (pi.PropertyType == typeof(System.Nullable<short>))
                    _newDataTable.Columns.Add(pi.Name, typeof(short));
                else if (pi.PropertyType == typeof(System.Nullable<decimal>))
                    _newDataTable.Columns.Add(pi.Name, typeof(decimal));
                else if (pi.PropertyType == typeof(System.Nullable<DateTime>))
                    _newDataTable.Columns.Add(pi.Name, typeof(DateTime));
                else
                    _newDataTable.Columns.Add(pi.Name, typeof(string));//hack: change to original datatype// pi.PropertyType);
            }

            //Populate the table
            foreach (T item in collection)
            {
                DataRow _newDataRow = _newDataTable.NewRow();
                _newDataRow.BeginEdit();
                foreach (PropertyInfo pi in _propInfo)
                    _newDataRow[pi.Name] = (pi.GetValue(item, null) == null) ?
                        DBNull.Value :
                        pi.GetValue(item, null);
                _newDataRow.EndEdit();
                _newDataTable.Rows.Add(_newDataRow);
            }

            return _newDataTable;
        }
        /// <summary>
        /// Pass a Data Table to add extra column from a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="sender"></param>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-07-07<br/>
        /// </remarks>
        public static void AddDataTableSchema<T>(ref DataTable sender, IEnumerable<T> collection)
        {
            if (collection == null) return;
            //retrieve the type passed into method
            Type impliedType = typeof(T);
            if (sender == null) sender = new DataTable(impliedType.Name);

            //Get an array of the Type's properties
            PropertyInfo[] propInfo = impliedType.GetProperties();

            //Create the columns in the DataTable
            foreach (PropertyInfo pi in propInfo)
            {
                if (pi.PropertyType == typeof(System.Nullable<short>))
                    sender.Columns.Add(impliedType.Name+"_"+pi.Name, typeof(short));
                else if (pi.PropertyType == typeof(System.Nullable<decimal>))
                    sender.Columns.Add(impliedType.Name + "_" + pi.Name, typeof(decimal));
                else if (pi.PropertyType == typeof(System.Nullable<DateTime>))
                    sender.Columns.Add(impliedType.Name + "_" + pi.Name, typeof(DateTime));
                else
                    sender.Columns.Add(impliedType.Name + "_" + pi.Name, typeof(string));//hack: change to original datatype// pi.PropertyType);
            }
        }
        /// <summary>
        /// Return a plural form for a singular word.
        /// </summary>
        /// <param name="singular"></param>
        /// <returns></returns>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-04-14<br/>
        /// </remarks>
        public static string ToPlural(string singular)
        {
            if (singular == null) return singular;
            if (singular.Length == 0) return singular;

            if (singular.LastIndexOf('y') == singular.Length - 1)
                return singular.Substring(0, singular.Length - 1) + "ies";
            else
                return singular + "s";
        }
        #endregion

        #region Sorting
        /// <summary>
        /// Sort the element of an array with buble sort.
        /// </summary>
        /// <param name="sender"></param>
        public static void BubbleSort(int[] sender)
        {
            for (int i = 1; i < sender.Length; i++)
            {
                for (int j = 0; j < sender.Length - 1; j++)
                {
                    if (sender[j] > sender[j + 1])
                        Swap(sender, j, j + 1);
                }
            }//end loops			
        }
        /// <summary>
        /// Swaop 2 element of an array.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private static void Swap(int[] sender, int first, int second)
        {
            int hold = sender[first];
            sender[first] = sender[second];
            sender[second] = hold;
        }
        /// <summary>
        /// Sort the element of an array with buble sort.
        /// </summary>
        /// <param name="sender"></param>
        public static void BubbleSort(string[] sender)
        {
            for (int i = 1; i < sender.Length; i++)
            {
                for (int j = 0; j < sender.Length - 1; j++)
                {
                    if (sender[j].CompareTo(sender[j + 1]) > 0)
                        Swap(sender, j, j + 1);
                }//end loops
            }//end loops		
        }
        /// <summary>
        /// Swaop 2 element of an array.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private static void Swap(string[] sender, int first, int second)
        {
            string hold = sender[first];
            sender[first] = sender[second];
            sender[second] = hold;
        }
        /// <summary>
        /// Sort the element of an array with buble sort.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="repeated">True if want to keep repeated value otherwise false.</param>
        public static string[][] FilterSort(string[][] sender, bool repeated)
        {
            string[][] output = new string[][] { };


            //doing bubble sort
            for (int i = 1; i < sender.Length; i++)
            {
                for (int j = 0; j < sender.Length - 1; j++)
                {
                    if (sender[j][0].CompareTo(sender[j + 1][0]) > 0)
                        Swap(sender, j, j + 1);
                }//end loops	
            }//end loops


            //start add in arraylist and convert to string[][]
            ArrayList al = new ArrayList();
            if (sender.Length > 0) al.Add(sender[0]);
            for (int i = 1; i < sender.Length; i++)
            {
                if (!repeated)
                {
                    if (sender[i][0] != sender[i - 1][0])
                        al.Add(sender[i]);
                    else
                        continue;
                }
                else
                    al.Add(sender[i]);
            }

            output = new string[al.Count][];
            for (int i = 0; i < al.Count; i++)
                output[i] = (al[i] as string[]);

            return output;
        }
        /// <summary>
        /// Swaop 2 element of an array.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private static void Swap(string[][] sender, int first, int second)
        {
            string hold = sender[first][0];
            sender[first][0] = sender[second][0];
            sender[second][0] = hold;

            string val = sender[first][1];
            sender[first][1] = sender[second][1];
            sender[second][1] = val;
        }
        #endregion

        /// <summary>
        /// For pair value use especially one is display text
        /// another is the value will save into database(i.e. Combobox, DropdownList).
        /// Please assigne DisplayMember = "DisplayText", and ValueMember = "Value".
        /// Probabaly will replace by System.Collections.Generic.
        /// </summary>
        /// <remarks>
        /// <b>author</b>   yeang-shing.then
        /// <b>since</b>    2008-09-24
        /// <example>
        /// <code>
        /// DataManipulation.ListItem[] statusList = new DataManipulation.ListItem[statusText.Length];
        /// for (int i = 0; i < statusText.Length; i++)
        ///     statusList[i] = new DataManipulation.ListItem(statusText[i], i);
        /// comboBox1.DataSource = statusList;
        /// comboBox1.DisplayMember = "DisplayText";
        /// comboBox1.ValueMember = "Value";
        /// </code>
        /// </example>
        /// </remarks>
        public class ListItem
        {
            private string displayText;
            /// <summary>
            /// The text to display.
            /// </summary>
            public string DisplayText
            {
                get { return this.displayText; }
                set { this.displayText = value; }
            }
            private object value;
            /// <summary>
            /// Value kept behind and stored into database.
            /// </summary>
            public object Value
            {
                get { return this.value; }
                set { this.value = value; }
            }
            public ListItem(string displayText, object value)
            {
                this.displayText = displayText;
                this.value = value;
            }
        }

    }//end class Data


    /// <summary>
    /// Numbering usage (create a new transaction number, new line numbering in article).
    /// </summary>
    /// <remarks>
    /// start at 060220tys.
    /// end by 060223tys
    /// </remarks>
    public class Numbering
    {
        /// <summary>
        /// Numbering style.
        /// </summary>
        [Flags]
        public enum Style
        {
            /// <summary>
            /// In arabic display.
            /// </summary>
            Arabic,
            /// <summary>
            /// Simplified and traditional style.
            /// 1. (small letter)yi,er,san,..
            /// 2. (big letter) yi,er,san,..
            /// 3. jia, yi, bing, ding,..
            /// 4. zi, chou, yin, mao, cheng,...
            /// 5. 60 jia - combination of (3),(4).
            /// </summary>
            Chinese,
            /// <summary>
            /// a,b,c,...x,y,z, aa,ab,ac,... zy,zz..
            /// </summary>
            English,
            /// <summary>
            /// Alpha to omega.
            /// </summary>
            Greek,
            /// <summary>
            /// Hebrew language for Israel
            /// </summary>
            Hebrew,
            /// <summary>
            /// 1,2,3,...
            /// </summary>
            Numeric,
            /// <summary>
            /// i,ii,iii,iv,v,...
            /// </summary>
            Roman,
        }

        #region Fields
        /// <summary>
        /// First index of numbering part.
        /// </summary>
        private static int indexStart;
        /// <summary>
        /// End index of numbering part.
        /// </summary>
        private static int indexEnd;
        /// <summary>
        /// Check whether is a valid numeric style.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static bool IsInteger(string val)
        {
            if (val.Trim().Length == 0) return false;

            Regex objNotIntPattern = new Regex("[^0-9-]");
            Regex objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
            return !objNotIntPattern.IsMatch(val) && objIntPattern.IsMatch(val);
        }
        /// <summary>
        /// Numbering style choosen.
        /// </summary>
        private static Style style;
        /// <summary>
        /// A seperator string (eg. -,#, ().. ).
        /// </summary>
        private string seperator;
        #endregion

        /// <summary>
        /// Get a new numbering based on style.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="source"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static string GetNewNumbering(Numbering.Style style, string source, int increment)
        {
            string output = "";

            #region try a way to get the encoding from source, then no need supply Style ..
            /* StringBuilder sb = new StringBuilder(source);
				System.Globalization.StringInfo lStringInfo = new System.Globalization.StringInfo();

				// Creates and initializes a TextElementEnumerator for myString.
				System.Globalization.TextElementEnumerator myTEE = System.Globalization.StringInfo.GetTextElementEnumerator(source);

				System.Text.Encoding lEncoding; */
            //string ls_EncodeName = lEncoding.EncodingName;
            #endregion

            try
            {
                //get index start to numbering
                indexStart = -1;
                indexEnd = source.Length - 1;
                bool isRomanSmall = false;//determine conversion later
                for (int i = 0; i < source.Length; i++)
                {
                    string s = source.Substring(i, 1);
                    if (indexStart == -1)
                    {
                        #region get start index
                        if (style == Style.Numeric)
                        {
                            if (IsInteger(s))
                                indexStart = i;
                        }
                        else if (style == Style.Roman)
                        {
                            if (IsRoman(s))
                                indexStart = i;
                        }
                        else if (style == Style.Arabic)
                        {
                            if (IsArabic(s))
                                indexStart = i;
                        }
                        else if (style == Style.Greek)
                        {
                            if (IsGreek(s))
                                indexStart = i;
                        }
                        #endregion
                    }
                    else
                    {
                        #region get end index
                        if (style == Style.Numeric)
                        {
                            if (!IsInteger(s))
                            {
                                indexEnd = i - 1;
                                break;
                            }
                        }
                        else if (style == Style.Roman)
                        {
                            if (!IsRoman(s))
                            {
                                indexEnd = i - 1;
                                break;
                            }
                        }
                        else if (style == Style.Arabic)
                        {
                            if (!IsArabic(s))
                            {
                                indexEnd = i - 1;
                                break;
                            }
                        }
                        else if (style == Style.Greek)
                        {
                            if (!IsGreek(s))
                            {
                                indexEnd = i - 1;
                                break;
                            }
                        }
                        #endregion
                    }
                }//end loops


                string ls_Check = source.Substring(indexStart, indexEnd - indexStart + 1);

                if (style == Style.Roman)
                {
                    #region check big letter or small leter
                    if (ls_Check == ls_Check.ToLower())
                        isRomanSmall = true;
                    #endregion
                }

                int newNumber = 0;
                #region get a new numbering based on style selected
                switch (style)
                {
                    case Style.Numeric:
                        newNumber = Convert.ToInt32(ls_Check);
                        break;
                    case Style.Roman:
                        newNumber = ConvertRomanToNumeric(ls_Check);
                        break;
                    case Style.Arabic:
                        newNumber = ConvertArabicToNumeric(ls_Check);
                        break;
                }
                newNumber += increment;
                #endregion



                output += source.Substring(0, indexStart);
                #region Get number part
                switch (style)
                {
                    case Style.Numeric:
                        output += newNumber.ToString().PadLeft(indexEnd - indexStart + 1, '0');
                        break;
                    case Style.Roman:
                        if (isRomanSmall)
                            output += ConvertToRoman(newNumber).ToLower();
                        else
                            output += ConvertToRoman(newNumber).ToUpper();
                        break;
                    case Style.Arabic:
                        output += ConvertToArabic(newNumber);
                        break;
                    case Style.Greek:
                        output += GetNewGreek(ls_Check, increment);
                        break;
                }
                #endregion
                output += source.Substring(indexEnd + 1, source.Length - 1 - indexEnd);
                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }

        #region Roman
        /// <summary>
        /// Check whether is a valid roman numbering.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <remarks>
        /// I = 1, V = 5, X = 10, L = 50, C = 100, D = 500, M = 1000.
        /// </remarks>
        private static bool IsRoman(char source)
        {
            string[] romans = new string[] { "I", "V", "X", "L", "C", "D", "M" };
            foreach (string s in romans)
            {
                if (s == source.ToString().ToUpper())
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Check whether is a valid roman numbering.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <remarks>
        /// Make sure right value must smaller or equal with left value or only allow bigger once.
        /// </remarks>
        private static bool IsRoman(string sender)
        {
            string[] romans = new string[] { "I", "V", "X", "L", "C", "D", "M" };
            ArrayList al = new ArrayList();
            foreach (char c in sender)
            {
                if (!IsRoman(c)) return false;
                for (int i = 0; i < romans.Length; i++)
                {
                    if (c.ToString().ToUpper() == romans[i])
                        al.Add(i);
                }//end loops
            }//end loops

            //check bigger
            //if has bigger then not allow repeat at right
            int bigger = 0;
            int isEqual = 0;
            for (int i = 1; i < al.Count; i++)
            {
                if (Convert.ToInt32(al[i]) == Convert.ToInt32(al[i - 1]))
                    isEqual++;
                else if (Convert.ToInt32(al[i]) > Convert.ToInt32(al[i - 1]))
                    bigger++;
            }
            if (bigger > 1 || bigger > 0 && isEqual > 0) return false;

            //check repeat except 'M'
            int repeat = 0;
            for (int i = 0; i < romans.Length - 1; i++)
            {
                repeat = 0;//reset
                foreach (char c in sender)
                {
                    if (c.ToString().ToUpper() == romans[i])
                        repeat++;
                }

                if (repeat > 3) return false;
            }


            return true;
        }
        /// <summary>
        /// Convert numeric to roman style (got defect in 99 value).
        /// </summary>
        /// <param name="number">Any integer.</param>
        /// <returns></returns>
        /// <remarks>
        /// Left is subtract,
        /// Right is plus
        /// 
        /// 
        ///  1 = I,
        ///  2 = II,
        ///  3 = III,
        ///  4 = IV,
        ///  5 = V,
        ///  6 = VI,
        ///  .
        ///  .
        ///  9 = IX,
        /// 10 = X,
        /// 11 = XI,
        ///  .
        ///  .
        /// 20 = XX,
        /// 30 = XXX,
        /// 50 = L,
        ///  .
        ///  .
        /// 80 = LXXX,
        /// 90 = XC,
        /// 98 = XCVIII,
        /// 99 = IC,
        /// 100 = C,
        /// 500 = D,
        /// 1000 = M
        /// 
        /// </remarks>
        public static string ConvertToRoman(int number)
        {
            string output = "O";//mean zero in roman style

            try
            {

                /*
                 * eg.
                 *  32 = 3 x 10 + 2 x 1
                 *  99 = 1 x 100 - 1 x 1
                 * 160 = 1 x 100 + 1 x 50 + 1 x 10
				 
                 * */


                int indicator = 1;//I
                string[] romans = new string[7] { "I", "V", "X", "L", "C", "D", "M" };
                int[] indicators = new int[7] { 1, 5, 10, 50, 100, 500, 1000 };
                ArrayList al = new ArrayList();//store {index key,times}
                int upper = indicators.Length - 1;

            FindDevider:
                indicator = 1;
                for (int i = 1; i < upper + 1; i++)
                {
                    if (number < indicators[i] && Math.Ceiling(Convert.ToDouble(number / indicators[i])) == 0)
                    {
                        upper = i - 1;
                        indicator = indicators[i - 1];
                        al.Add(new int[2] { i - 1, Convert.ToInt32(Math.Ceiling(Convert.ToDouble(number / indicator))) });
                        break;
                    }

                    if (i == indicators.Length - 1)
                    {
                        upper = i;
                        indicator = indicators[i];
                        al.Add(new int[2] { i, Convert.ToInt32(Math.Ceiling(Convert.ToDouble(number / indicator))) });
                    }
                    System.Diagnostics.Debug.WriteLine(Math.Ceiling(Convert.ToDouble(number / indicators[i])));
                }//end loops

                if (indicator != 1 && number % indicator > 0) //if(number-indicator*Convert.ToInt32(al[al.Count-1]) > 0)
                {
                    number = number % indicator;
                    goto FindDevider;
                }

                //start join roman string
                for (int i = 0; i < al.Count; i++)
                {
                    if (i == 0) output = "";
                    string join = "";
                    if ((al[i] as int[])[1] >= 4)	//eg. VIIII -> IX
                        join += romans[(al[i] as int[])[0]] + romans[(al[i] as int[])[0] + 1];
                    else
                    {
                        for (int j = 0; j < (al[i] as int[])[1]; j++)
                            join += romans[(al[i] as int[])[0]].ToString();
                    }


                    output += join;
                    //eg. VIV -> IX
                    if (output.Length > 2
                        && output.Substring(output.Length - 1, 1) != output.Substring(output.Length - 2, 1)
                        && output.Substring(output.Length - 1, 1) == output.Substring(output.Length - 3, 1))
                    {
                        string before = output.Substring(0, output.Length - 3);
                        join = romans[(al[i] as int[])[0]] + romans[(al[i] as int[])[0] + 2];
                        output = before + join;
                    }
                }//end loops


                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        /// <summary>
        /// Convert roman style value to numeric.
        /// </summary>
        /// <param name="roman"></param>
        /// <returns></returns>
        public static int ConvertRomanToNumeric(string roman)
        {
            int output = 0;
            //make sure is a valid input
            if (!IsRoman(roman))
                return output;

            try
            {
                //mark previous value because there was impossible right value > left value
                //when there only minus case eg.
                // IV = 5-1
                // IC = 100-1
                int previous = 1000;

                string[] romans = new string[7] { "I", "V", "X", "L", "C", "D", "M" };
                int[] indicators = new int[7] { 1, 5, 10, 50, 100, 500, 1000 };
                foreach (char c in roman)
                {
                    for (int i = 0; i < romans.Length; i++)
                    {
                        if (c.ToString().ToUpper() == romans[i])
                        {
                            if (indicators[i] > previous)
                                output += indicators[i] - 2 * previous;
                            else
                                output += indicators[i];
                            previous = indicators[i];
                        }
                    }//end loops
                }//end loops


                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        #endregion

        #region Arabic
        /// <summary>
        /// Is a valid arabic character.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static bool IsArabic(string sender)
        {
            //0,1,2,3,4,5,6,7,8,9
            string[] arabics = new string[10] { "\u0660", "\u0661", "\u0662", "\u0663", "\u0664", "\u0665", "\u0666", "\u0667", "\u0668", "\u0669" };

            if (sender.Length == 1)
            {
                foreach (string s in arabics)
                {
                    if (sender == s)
                        return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Convert numeric value to arabic symbol.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <remarks>
        /// Arabic number just like number behavior,
        /// only different is it display reverse, from right to left.
        /// </remarks>
        public static string ConvertToArabic(int number)
        {
            string output = "";

            //0,1,2,3,4,5,6,7,8,9
            string[] arabics = new string[10] { "\u0660", "\u0661", "\u0662", "\u0663", "\u0664", "\u0665", "\u0666", "\u0667", "\u0668", "\u0669" };
            string numberString = number.ToString();
            for (int i = numberString.Length - 1; i >= 0; i--)
                output += arabics[Convert.ToInt32(numberString.Substring(i, 1))];

            return output;
        }
        /// <summary>
        /// Convert Arabic numbering to numeric.
        /// </summary>
        /// <param name="arabic"></param>
        /// <returns></returns>
        public static int ConvertArabicToNumeric(string arabic)
        {
            int output = 0;

            string numberString = "";
            //0,1,2,3,4,5,6,7,8,9
            string[] arabics = new string[10] { "\u0660", "\u0661", "\u0662", "\u0663", "\u0664", "\u0665", "\u0666", "\u0667", "\u0668", "\u0669" };
            for (int i = arabic.Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < arabics.Length; j++)
                {
                    if (arabic.Substring(i, 1) == arabics[j])
                        numberString += j.ToString();
                }//end loops
            }//end loops

            output = Convert.ToInt32(numberString);
            return output;
        }

        #endregion

        #region Greek
        /// <summary>
        /// Is a valid greek character.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static bool IsGreek(string sender)
        {
            string[] greeks = new string[24 + 25]{
													   "\u0391","\u0392","\u0393","\u0394","\u0395",
													   "\u0396","\u0397","\u0398","\u0399","\u039A",
													   "\u039B","\u039C","\u039D","\u039F","\u03A0",
													   "\u03A1","\u03A2","\u03A3","\u03A4","\u03A5",
													   "\u03A6","\u03A7","\u03A8","\u03A9",
		
													   "\u03B1","\u03B2","\u03B3","\u03B4","\u03B5",
													   "\u03B6","\u03B7","\u03B8","\u03B9","\u03BA",
													   "\u03BB","\u03BC","\u03BD","\u03BE","\u03BF",
													   "\u03C0","\u03C1","\u03C2","\u03C3","\u03C4",
													   "\u03C5","\u03C6","\u03C7","\u03C8","\u03C9"
												   };
            if (sender.Length == 1)
            {
                for (int i = 0; i < greeks.Length; i++)
                {
                    if (sender == greeks[i])
                        return true;
                }
            }


            return false;
        }
        /// <summary>
        /// Get a new greek numbering.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static string GetNewGreek(string sender, int increment)
        {
            string output = "";

            try
            {
                //alpha ~ omega
                string[] greeks = new string[24 + 25]{
														   "\u0391","\u0392","\u0393","\u0394","\u0395",
														   "\u0396","\u0397","\u0398","\u0399","\u039A",
														   "\u039B","\u039C","\u039D","\u039F","\u03A0",
														   "\u03A1","\u03A2","\u03A3","\u03A4","\u03A5",
														   "\u03A6","\u03A7","\u03A8","\u03A9",
		
														   "\u03B1","\u03B2","\u03B3","\u03B4","\u03B5",
														   "\u03B6","\u03B7","\u03B8","\u03B9","\u03BA",
														   "\u03BB","\u03BC","\u03BD","\u03BE","\u03BF",
														   "\u03C0","\u03C1","\u03C2","\u03C3","\u03C4",
														   "\u03C5","\u03C6","\u03C7","\u03C8","\u03C9"
													   };

                string[] bigGreeks = new string[24]{
														   "\u0391","\u0392","\u0393","\u0394","\u0395",
														   "\u0396","\u0397","\u0398","\u0399","\u039A",
														   "\u039B","\u039C","\u039D","\u039F","\u03A0",
														   "\u03A1","\u03A2","\u03A3","\u03A4","\u03A5",
														   "\u03A6","\u03A7","\u03A8","\u03A9",
					};

                string[] smallGreeks = new string[25]{
															 "\u03B1","\u03B2","\u03B3","\u03B4","\u03B5",
															 "\u03B6","\u03B7","\u03B8","\u03B9","\u03BA",
															 "\u03BB","\u03BC","\u03BD","\u03BE","\u03BF",
															 "\u03C0","\u03C1","\u03C2","\u03C3","\u03C4",
															 "\u03C5","\u03C6","\u03C7","\u03C8","\u03C9"
														 };

                ArrayList al = new ArrayList();//hold greeks[] index
                for (int i = 0; i < sender.Length; i++)
                {
                    for (int j = 0; j < greeks.Length; j++)
                    {
                        if (sender.Substring(i, 1) == greeks[j])
                            al.Add(j);
                    }
                }//end loops

                int[] currGreeks = new int[al.Count];
                int[] newGreeks = new int[al.Count];
                for (int i = 0; i < currGreeks.Length; i++)
                    currGreeks[i] = Convert.ToInt32(al[i]);

                int hold = 0;
                bool addOne = false;//add previous digit
                for (int i = currGreeks.Length - 1; i >= 0; i--)
                {
                    if (i < currGreeks.Length - 1) increment = 0;
                    if (currGreeks[i] >= 24)
                    {
                        #region small letter
                        if (addOne)
                            hold = currGreeks[i] + increment + 1;
                        else
                            hold = currGreeks[i] + increment;
                        hold -= 24;

                        addOne = false;//reset
                        if (hold != hold % 25)
                            addOne = true;
                        hold = hold % 25;

                        newGreeks[i] = hold + 24;
                        #endregion
                    }
                    else
                    {
                        #region big letter
                        if (addOne)
                            hold = currGreeks[i] + increment + 1;
                        else
                            hold = currGreeks[i] + increment;

                        addOne = false;//reset
                        if (hold != hold % 24)
                            addOne = true;
                        hold = hold % 24;

                        newGreeks[i] = hold;
                        #endregion
                    }
                }//end loops



                for (int i = 0; i < newGreeks.Length; i++)
                {
                    if (newGreeks[i] >= 24)
                        output += smallGreeks[newGreeks[i] - 24];
                    else
                        output += bigGreeks[newGreeks[i]];
                }

                if (addOne)
                    return output = bigGreeks[0] + output;//very seldom case
                else
                    return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        #endregion

        #region English words
        private static string GetNumberToWords(string as_amt)
        {
            string text1 = "";
            string text2 = "";
            int num1 = Convert.ToInt32(as_amt.Substring(1, 2));
            int num2 = Convert.ToInt32(as_amt.Substring(2, 1));
            int num3 = Convert.ToInt32(as_amt.Substring(1, 1));
            int num4 = Convert.ToInt32(as_amt.Substring(0, 1));
            if (num1 < 20)
            {
                text1 = GetTeen(num1);
            }
            else
            {
                text1 = GetTeen(num2);
                if ((num2 > 0) && (num3 > 0))
                {
                    text2 = "-";
                }
                text1 = GetTeens(num3) + text2 + text1.TrimStart(null);
            }
            if (num4 <= 0)
            {
                return text1;
            }
            if (text1.Equals(string.Empty))
            {
                return (GetTeen(num4) + " HUNDRED" + text1);
            }
            return (GetTeen(num4) + " HUNDRED AND" + text1);
        }
        public static string GetNumericFromString(string as_string)
        {
            string text1 = "";
            if (as_string.Trim() == "")
            {
                throw new Exception("String not contain any numeric string!");
            }
            for (int num1 = 0; num1 < as_string.Length; num1++)
            {
                if (char.IsNumber(as_string, num1))
                {
                    text1 = text1 + as_string.Substring(num1, 1);
                }
            }
            return text1;
        }
        //		public string AmtInWords(decimal ad_amt, string as_coID, string as_currID, string as_letterCase)
        //		{
        //			string text1;
        //			SqlConnection connection1 = null;
        //			SqlCommand command1 = null;
        //			SqlDataReader reader1 = null;
        //			Db database1 = new Db();
        //			if (ad_amt == new decimal(0))
        //			{
        //				return "NIL";
        //			}
        //			ad_amt = Math.Abs(ad_amt);
        //			try
        //			{
        //				string[] textArray3;
        //				connection1 = database1.GetConnection();
        //				string text12 = "";
        //				string text13 = "";
        //				string text14 = "";
        //				string text15 = "";
        //				command1 = database1.OpenConn(connection1, "sm_mtn_curr_retrieve");
        //				command1.Parameters.Add(new SqlParameter("@CoID", as_coID));
        //				command1.Parameters.Add(new SqlParameter("@CurrID", as_currID));
        //				reader1 = command1.ExecuteReader();
        //				if (reader1.Read())
        //				{
        //					text12 = reader1["CurrDesc"].ToString();
        //					text13 = reader1["CurrRem1"].ToString();
        //				}
        //				if (reader1 != null)
        //				{
        //					reader1.Close();
        //				}
        //				if (this.StringToEmpty(text13) == "")
        //				{
        //					text13 = "cent";
        //				}
        //				text14 = text12;
        //				text15 = text13 + "s";
        //				string text2 = ad_amt.ToString("000000000000000.00000000");
        //				string text3 = text2.Substring(0, 3);
        //				string text4 = text2.Substring(3, 3);
        //				string text5 = text2.Substring(6, 3);
        //				string text6 = text2.Substring(9, 3);
        //				string text7 = text2.Substring(12, 3);
        //				string text16 = "";
        //				decimal num5 = this.StringToDecimal("0." + text2.Substring(0x10, 8));
        //				text16 = num5.ToString("0.00######");
        //				text16 = text16.Substring(2, text16.Length - 2).PadLeft(9, '0');
        //				string text8 = text16.Substring(0, 3);
        //				string text9 = text16.Substring(3, 3);
        //				string text10 = text16.Substring(6, 3);
        //				text1 = "";
        //				if (Convert.ToInt32(text2.Substring(0, 15)) == 0)
        //				{
        //					text12 = "";
        //				}
        //				if (Convert.ToInt32(text2.Substring(text2.Length - 8, 8)) == 0)
        //				{
        //					text13 = "";
        //				}
        //				if (Convert.ToInt32(text2.Substring(0, 15)) > 1)
        //				{
        //					text12 = text14.Trim();
        //				}
        //				else
        //				{
        //					text12 = text12.Trim();
        //				}
        //				if (Convert.ToInt32(text2.Substring(text2.Length - 8, 8)) > 1)
        //				{
        //					text13 = text15.Trim();
        //				}
        //				else
        //				{
        //					text13 = text13.Trim();
        //				}
        //				string text11 = "";
        //				if ((text12 != string.Empty) && (text13 != string.Empty))
        //				{
        //					text11 = " AND ";
        //				}
        //				if (Convert.ToInt32(text3) > 0)
        //				{
        //					text1 = text1 + this.GetNumberToWords(text3) + " TRILLION";
        //				}
        //				if (Convert.ToInt32(text4) > 0)
        //				{
        //					text1 = text1 + this.GetNumberToWords(text4) + " BILLION";
        //				}
        //				if (Convert.ToInt32(text5) > 0)
        //				{
        //					text1 = text1 + this.GetNumberToWords(text5) + " MILLION";
        //				}
        //				if (Convert.ToInt32(text6) > 0)
        //				{
        //					text1 = text1 + this.GetNumberToWords(text6) + " THOUSAND";
        //				}
        //				if (Convert.ToInt32(text7) > 0)
        //				{
        //					text1 = text1 + this.GetNumberToWords(text7);
        //				}
        //				text1 = text12 + " " + text1;
        //				string text17 = "";
        //				if (Convert.ToInt32(text8) > 0)
        //				{
        //					text17 = text17 + this.GetNumberToWords(text8) + " MILLION";
        //				}
        //				if (Convert.ToInt32(text9) > 0)
        //				{
        //					text17 = text17 + this.GetNumberToWords(text9) + " THOUSAND";
        //				}
        //				if (Convert.ToInt32(text10) > 0)
        //				{
        //					text17 = text17 + this.GetNumberToWords(text10);
        //				}
        //				text1 = text1.Trim();
        //				text17 = text17.Trim();
        //				if (!text13.Equals(string.Empty))
        //				{
        //					string text18 = text1;
        //					textArray3 = new string[5] { text18, text11, text13, " ", text17 } ;
        //					text1 = string.Concat(textArray3);
        //				}
        //				text1 = text1 + " ONLY";
        //				if (as_letterCase.Trim().ToUpper() == "U")
        //				{
        //					return text1.ToUpper();
        //				}
        //				if (as_letterCase.Trim().ToUpper() == "L")
        //				{
        //					return text1.ToLower();
        //				}
        //				if (as_letterCase.Trim().ToUpper() != "P")
        //				{
        //					return text1;
        //				}
        //				char[] chArray1 = new char[1] { ' ' } ;
        //				string[] textArray1 = text1.ToLower().Split(chArray1);
        //				if (textArray1.Length <= 0)
        //				{
        //					return text1;
        //				}
        //				for (int num1 = 0; num1 < textArray1.Length; num1++)
        //				{
        //					if (textArray1[num1].Trim() != "")
        //					{
        //						chArray1 = new char[1] { '-' } ;
        //						string[] textArray2 = textArray1[num1].Split(chArray1);
        //						if (textArray2.Length > 0)
        //						{
        //							for (int num2 = 0; num2 < textArray2.Length; num2++)
        //							{
        //								textArray2[num2] = textArray2[num2].Substring(0, 1).ToUpper() + textArray2[num2].Substring(1, textArray2[num2].Length - 1).ToString();
        //							}
        //							textArray1[num1] = "";
        //							for (int num3 = 0; num3 < textArray2.Length; num3++)
        //							{
        //								IntPtr ptr1;
        //								(textArray3 = textArray1)[(int) (ptr1 = (IntPtr) num1)] = textArray3[(int) ptr1] + textArray2[num3].ToString() + "-";
        //							}
        //							textArray1[num1] = textArray1[num1].Substring(0, textArray1[num1].Length - 1);
        //						}
        //						textArray1[num1] = textArray1[num1].Substring(0, 1).ToUpper() + textArray1[num1].Substring(1, textArray1[num1].Length - 1).ToString();
        //					}
        //				}
        //				text1 = "";
        //				for (int num4 = 0; num4 < textArray1.Length; num4++)
        //				{
        //					text1 = text1 + textArray1[num4].ToString().Trim() + " ";
        //				}
        //				text1 = text1.Trim();
        //			}
        //			catch (Exception exception1)
        //			{
        //				throw exception1;
        //			}
        //			finally
        //			{
        //				if (reader1 != null)
        //				{
        //					reader1.Close();
        //				}
        //				if ((connection1 != null) && (connection1.State == ConnectionState.Open))
        //				{
        //					connection1.Close();
        //				}
        //			}
        //			return text1;
        //		}
        public static string GetTeen(int li_number)
        {
            string[] textArray2 = new string[20] { 
														 "", " ONE", " TWO", " THREE", " FOUR", " FIVE", " SIX", " SEVEN", " EIGHT", " NINE", " TEN", " ELEVEN", " TWELVE", " THIRTEEN", " FOURTEEN", " FIFTEEN", 
														 " SIXTEEN", " SEVENTEEN", " EIGHTEEN", " NINETEEN"
													 };
            string[] textArray1 = textArray2;
            if (li_number > (textArray1.Length - 1))
            {
                Debug.WriteLine("Invalid value : " + li_number.ToString() + "!", "Error In Function GetTeen()");
            }
            return textArray1[li_number];
        }
        public static string GetTeens(int li_number)
        {
            string[] textArray2 = new string[11] { "", " ONE", " TWENTY", " THIRTY", " FORTY", " FIFTY", " SIXTY", " SEVENTY", " EIGHTY", " NINETY", " HUNDRED" };
            string[] textArray1 = textArray2;
            if (li_number > (textArray1.Length - 1))
            {
                Debug.WriteLine("Invalid value : " + li_number.ToString() + "!", "Error In Function GetTeens()");
            }
            return textArray1[li_number];
        }
        /// <summary>
        /// Add th in the ending of number.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string Addth(int number)
        {
            string output = number.ToString();
            int lastDigit = Convert.ToInt32(number.ToString().Substring(number.ToString().Length - 1, 1));
            switch (lastDigit)
            {
                case 1:
                    if (Convert.ToInt32(number.ToString().Substring(number.ToString().Length - 2, 1)) == 1)
                        output += "th";
                    else
                        output += "st";
                    break;
                case 2:
                    output += "nd";
                    break;
                case 3:
                    output += "rd";
                    break;
                default:
                    output += "th";
                    break;
            }


            return output;
        }
        #endregion

        #region Chinese part
        /// <summary>
        /// Is a valid chinese small character (normally is this or default type).
        /// </summary>
        private static bool isChineseSmallLetter;
        /// <summary>
        /// Is chinese big letter character.
        /// </summary>
        private static bool isChineseBigLetter;
        /// <summary>
        /// Is chinese tian gan character.
        /// </summary>
        private static bool isChineseTianGan;
        /// <summary>
        /// Is chinese di zhi character.
        /// </summary>
        private static bool isChineseDiZhi;
        /// <summary>
        /// Check whether contain a valid chinese character and determine which kind of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static bool IsChinese(string sender)
        {
            /*
             * 0 = "\u96F6",
             * 1 = "\u4E00",
             * 2 = "\u4E8C",
             * 3 = "\u4E09",
             * 4 = "\u56DB",
             * 5 = "\u4E94",
             * 6 = "\u516D",
             * 7 = "\u4E03",
             * 8 = "\u516B",
             * 9 = "\u4E5D",
             * 10 = "\u5341",
             * 100 = "\u767E",
             * 1000 = "\u5343",
             * 10000 = "\u4E07",
             * 10000 0000 = "\u4EBF",
             * 10000 0000 0000 = "\u5146".
             * 
             * */
            string[] smallChineses = new string[10] { "\u96F6", "\u4E00", "\u4E8C", "\u4E09", "\u56DB", "\u4E94", "\u516D", "\u4E03", "\u516B", "\u4E5D" };


            /*
             * 0 = "\u96F6",
             * 1 = "\u58F9",
             * 2 = "\u8D30",
             * 3 = "\u53C1",
             * 4 = "\u8086",
             * 5 = "\u4F0D",
             * 6 = "\u9646",
             * 7 = "\u67D2",
             * 8 = "\u634C",
             * 9 = "\u7396",
             * 10 = "\u62FE",
             * 100 = "\u4F70",
             * 1000 = "\u4EDF",
             * 10000 = "\u4E07",
             * 10000 0000 = "\u4EBF",
             * 10000 0000 0000 = "\u5146".
             * 
             * */
            string[] bigChineses = new string[10] { "\u96F6", "\u58F9", "\u8D30", "\u53C1", "\u8086", "\u4F0D", "\u9646", "\u67D2", "\u634C", "\u7396" };
            for (int i = 0; i < sender.Length; i++)
            {
                foreach (string s in bigChineses)
                {
                    if (s == sender.Substring(i, 1))
                    {
                        isChineseBigLetter = true;
                        return true;
                    }
                }
            }//end loops

            /*
             * 1 = "\u7532", (jia3)
             * 2 = "\u4E59", (yi3)
             * 3 = "\u4E19", (bing3)
             * 4 = "\u4E01", (ding1)
             * 5 = "\u620A", (wu4)
             * 6 = "\u5DF1", (ji3)
             * 7 = "\u5E9A", (geng1)
             * 8 = "\u8F9B", (xin1)
             * 9 = "\u58EC", (ren2)
             * 10 = "\u7678", (gui3)
             * 
             * 
             * */
            string[] tianGans = new string[10] { "\u7532", "\u4E59", "\u4E19", "\u4E01", "\u620A", "\u5DF1", "\u5E9A", "\u8F9B", "\u58EC", "\u7678" };
            for (int i = 0; i < sender.Length; i++)
            {
                foreach (string s in tianGans)
                {
                    if (s == sender.Substring(i, 1))
                    {
                        isChineseTianGan = true;
                        return true;
                    }
                }
            }//end loops


            /* 
             * 1 = "\u5B50" (zhi3),
             * 2 = "\u4E11" (zhou3),
             * 3 = "\u5BC5" (yin3),
             * 4 = "\u536F" (mao3),
             * 5 = "\u8FB0" (cheng2),
             * 6 = "\u5DF3" (shi4),
             * 7 = "\u5348" (wu3),
             * 8 = "\u672A" (wei4),
             * 9 = "\u7533" (shen1),
             * 10 = "\u9149" (you3),
             * 11 = "\u620D" (shu4),
             * 12 = "\u4EA5" (hai4).
             * 
             * */
            string[] diZhis = new string[12] { "\u5B50", "\u4E11", "\u5BC5", "\u536F", "\u8FB0", "\u5DF3", "\u5348", "\u672A", "\u7533", "\u9149", "\u620D", "\u4EA5" };
            for (int i = 0; i < sender.Length; i++)
            {
                foreach (string s in diZhis)
                {
                    if (s == sender.Substring(i, 1))
                    {
                        isChineseDiZhi = true;
                        return true;
                    }
                }
            }//end loops

            return false;
        }
        /// <summary>
        /// Convert to chinese numbering.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string ConvertToChinese(int number)
        {
            string output = "";

            try
            {


                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        /// <summary>
        /// Get a chinese correspond digit character.
        /// </summary>
        /// <param name="digit">Digit eg. 2 to get ten, 3 to get hundred.</param>
        /// <returns></returns>
        private static string GetChineseDigit(int digit)
        {
            string output = "";

            // 10, 100, 1000, 10000, 10000 0000, 10000 0000 0000
            string ten = "\u62FE";
            string hundred = "\u4F70";
            string thousand = "\u4EDF";
            string wan = "\u4E07";
            string yi = "\u4EBF";
            string zhao = "\u5146";

            #region shortcut and for long digits use.. fail
            /* switch(digit%5)
				{
					case 2:
						output += ten;
						break;
					case 3:
						output += hundred;
						break;
					case 4:
						output += thousand;
						break;
				}

				int countWan = Convert.ToInt32( Math.Ceiling(digit/4) );
				string suffix = "";
				for(int i=countWan-1;i>=0;i--)
				{
					suffix += wan;
					if(suffix==wan+wan)
						suffix = suffix.Replace(wan+wan,yi);
					if(suffix==wan+yi)
						suffix = suffix.Replace(wan+yi,zhao);
				}

				output = output+suffix; */
            #endregion


            #region hardcode method
            switch (digit)
            {
                case 2:
                    output += ten;
                    break;
                case 3:
                    output += hundred;
                    break;
                case 4:
                    output += thousand;
                    break;
                case 5:
                    output += wan;
                    break;
                case 6:
                    output += ten + wan;
                    break;
                case 7:
                    output += hundred + wan;
                    break;
                case 8:
                    output += thousand + wan;
                    break;
                case 9:
                    output += yi;
                    break;
                case 10:
                    output += ten + yi;
                    break;
                case 11:
                    output += hundred + yi;
                    break;
                case 12:
                    output += thousand + yi;
                    break;
                case 13:
                    output += zhao;
                    break;
                case 14:
                    output += ten + zhao;
                    break;
                case 15:
                    output += hundred + zhao;
                    break;
                case 16:
                    output += thousand + zhao;
                    break;
            }
            #endregion

            return output;
        }
        /// <summary>
        /// Convert to chinese big character.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ConvertToChineseWords(int number)
        {
            string output = "";

            try
            {
                //0,1,2,3,4,5,6,7,8,9
                string[] bigChineses = new string[10] { "\u96F6", "\u58F9", "\u8D30", "\u53C1", "\u8086", "\u4F0D", "\u9646", "\u67D2", "\u634C", "\u7396" };
                string wan = "\u4E07";//10,000
                string yi = "\u4EBF";//100,000,000
                string zhao = "\u5146";//100,000,000,000

                string numberString = number.ToString();
                string[] numbers = new string[Convert.ToInt32(Math.Ceiling(Convert.ToDouble(numberString.Length / 4)) + 1)];
                int pos = numberString.Length - 1 - 3;
                for (int i = numbers.Length - 1; i >= 0; i--)
                {
                    if (i == 0)
                        numbers[i] = numberString.Substring(Math.Max(pos, 0), numberString.Length % 4);
                    else
                        numbers[i] = numberString.Substring(Math.Max(pos, 0), 4);
                    pos -= 4;
                }//end loops


                for (int i = 0; i < numbers.Length; i++)
                {
                    for (int j = 0; j < numbers[i].Length; j++)
                    {
                        int hold = Convert.ToInt32(numbers[i].Substring(j, 1));
                        if (hold == 0)
                        {
                            if (output.Substring(output.Length - 1, 1) != bigChineses[0])
                                output += bigChineses[hold];
                        }
                        else
                            output += bigChineses[hold] + GetChineseDigit(numbers[i].Length - j);
                    }//end loops each part of 4 digit

                    //handle digit
                    switch (numbers.Length - 1 - i)
                    {
                        case 1:
                            output += wan;
                            break;
                        case 2:
                            output += yi;
                            break;
                        case 3:
                            output += zhao;
                            break;
                    }
                }//end loops


                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        /// <summary>
        /// Convert currency to chinese big character (usually bank use).
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ConvertCurrencyToChineseWords(decimal number)
        {
            string output = "";

            string yen = "\u5143";
            string only = "\u6B63";
            string cent = "\u89D2";

            try
            {
                string[] numbers = number.ToString().Split('.');
                string prefix = ConvertToChineseWords(Convert.ToInt32(numbers[0]));
                output += prefix + yen;
                if (numbers.Length > 1)
                {
                    //handle 10 cent..
                    string suffix = ConvertToChineseWords(Convert.ToInt32(numbers[1]));
                    output += suffix + cent;
                }

                output += only;
                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        /// <summary>
        /// Get a new chinese numbering based on original kind.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static string GetNewChinese(string sender, int increment)
        {
            string output = "";

            try
            {
                string[] smallChineses = new string[10] { "\u96F6", "\u4E00", "\u4E8C", "\u4E09", "\u56DB", "\u4E94", "\u516D", "\u4E03", "\u516B", "\u4E5D" };
                string[] bigChineses = new string[10] { "\u96F6", "\u58F9", "\u8D30", "\u53C1", "\u8086", "\u4F0D", "\u9646", "\u67D2", "\u634C", "\u7396" };
                string[] tianGans = new string[10] { "\u7532", "\u4E59", "\u4E19", "\u4E01", "\u620A", "\u5DF1", "\u5E9A", "\u8F9B", "\u58EC", "\u7678" };
                string[] diZhis = new string[12] { "\u5B50", "\u4E11", "\u5BC5", "\u536F", "\u8FB0", "\u5DF3", "\u5348", "\u672A", "\u7533", "\u9149", "\u620D", "\u4EA5" };

                bool addOne = false;
                int n = -1;//new index
                for (int i = sender.Length - 1; i >= 0; i--)
                {
                    string s = sender.Substring(i, 1);

                    #region check if smallchinese
                    for (int j = 0; j < smallChineses.Length; j++)
                    {
                        if (s == smallChineses[j])
                        {
                            if (addOne && i < sender.Length - 1)
                            {
                                n = j + increment + 1;
                                addOne = false;//reset
                            }
                            else
                                n = j + increment;
                            if (n != n % smallChineses.Length) addOne = true;
                            output += smallChineses[n % smallChineses.Length];
                            if (addOne && i == 0) output = smallChineses[1] + output;
                            isChineseSmallLetter = true;
                            break;
                        }
                    }
                    if (isChineseSmallLetter) continue;
                    #endregion

                    #region check if bigChineses
                    for (int j = 0; j < bigChineses.Length; j++)
                    {
                        if (s == bigChineses[j])
                        {
                            if (addOne && i < sender.Length - 1)
                            {
                                n = j + increment + 1;
                                addOne = false;//reset
                            }
                            else
                                n = j + increment;
                            if (n != n % bigChineses.Length) addOne = true;
                            output += bigChineses[n % bigChineses.Length];
                            if (addOne && i == 0) output = bigChineses[1] + output;
                            isChineseBigLetter = true;
                            break;
                        }
                    }
                    if (isChineseBigLetter) continue;
                    #endregion

                    #region check if tianGans
                    for (int j = 0; j < tianGans.Length; j++)
                    {
                        if (s == tianGans[j])
                        {
                            if (addOne && i < sender.Length - 1)
                            {
                                n = j + increment + 1;
                                addOne = false;//reset
                            }
                            else
                                n = j + increment;
                            if (n != n % tianGans.Length) addOne = true;
                            output += tianGans[n % tianGans.Length];
                            //if(addOne && i==0) output = tianGans[0]+output;
                            isChineseTianGan = true;
                            break;
                        }
                    }
                    if (isChineseTianGan) continue;
                    #endregion

                    #region check if diZhis
                    for (int j = 0; j < diZhis.Length; j++)
                    {
                        if (s == diZhis[j])
                        {
                            if (addOne && i < sender.Length - 1)
                            {
                                n = j + increment + 1;
                                addOne = false;//reset
                            }
                            else
                                n = j + increment;
                            if (n != n % diZhis.Length) addOne = true;
                            output += diZhis[n % diZhis.Length];
                            //if(addOne && i==0) output = diZhis[0]+output;
                            isChineseDiZhi = true;
                            break;
                        }
                    }
                    if (isChineseDiZhi) continue;
                    #endregion


                }//end loops

                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        /// <summary>
        /// Get a new 6jia numbering.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static string GetNew6Jia(string sender, int increment)
        {
            string output = "";
            if (sender.Trim().Length != 2) return output;

            try
            {
                string[] tianGans = new string[10] { "\u7532", "\u4E59", "\u4E19", "\u4E01", "\u620A", "\u5DF1", "\u5E9A", "\u8F9B", "\u58EC", "\u7678" };
                string[] diZhis = new string[12] { "\u5B50", "\u4E11", "\u5BC5", "\u536F", "\u8FB0", "\u5DF3", "\u5348", "\u672A", "\u7533", "\u9149", "\u620D", "\u4EA5" };

                for (int i = 0; i < tianGans.Length; i++)
                {
                    if (sender.Substring(0, 1) == tianGans[i])
                        output += tianGans[(i + increment) % 10];
                }

                for (int i = 0; i < diZhis.Length; i++)
                {
                    if (sender.Substring(1, 1) == diZhis[i])
                        output += diZhis[(i + increment) % 12];
                }


                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        /// <summary>
        /// Get a correspond chinese year in lunar.
        /// </summary>
        /// <param name="year">Gregorian year.</param>
        /// <returns></returns>
        public string Get6JiaYear(int year)
        {
            string output = "";

            try
            {
                /*
                 * if year%60=26 -> 2006 a.c.
                 * tianGans[3]+diZhis[10]
                 * 
                 * if year%60=0 -> 1980 a.c.
                 * tianGans[6]+diZhis[8]
                 * 
                 * */

                string[] tianGans = new string[10] { "\u7532", "\u4E59", "\u4E19", "\u4E01", "\u620A", "\u5DF1", "\u5E9A", "\u8F9B", "\u58EC", "\u7678" };
                string[] diZhis = new string[12] { "\u5B50", "\u4E11", "\u5BC5", "\u536F", "\u8FB0", "\u5DF3", "\u5348", "\u672A", "\u7533", "\u9149", "\u620D", "\u4EA5" };

                int i = Math.Abs(year - 1980);
                int t = 6;
                int d = 8;
                output = tianGans[(t + i) % 10] + diZhis[(d + i) % 12];

                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
        #endregion

    }//end class Numbering


    /// <summary>
    /// Rounding tool.
    /// </summary>
    public class Round
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Round()
        {
        }
        public decimal ToDecimal(decimal ad_decimal, int ai_decimal)
        {
            if (ad_decimal.ToString() == "")
            {
                ad_decimal = new decimal(0);
                return ad_decimal;
            }
            if (ad_decimal < new decimal(0))
            {
                ad_decimal -= new decimal(1, 0, 0, false, 10);
            }
            else
            {
                ad_decimal += new decimal(1, 0, 0, false, 10);
            }
            ad_decimal = decimal.Round(ad_decimal, ai_decimal);
            return ad_decimal;
        }
        public string ToFormatDecimal(string as_display, int ai_dp)
        {
            string text1 = "";
            if (as_display == "Y")
            {
                if (ai_dp == 0)
                {
                    return "##,###,###,##0";
                }
                if (ai_dp == 1)
                {
                    return "###,###,##0.0";
                }
                if (ai_dp != 2)
                {
                    if (ai_dp == 3)
                    {
                        return "##,###,##0.000";
                    }
                    if (ai_dp == 4)
                    {
                        return "#,###,##0.0000";
                    }
                    if (ai_dp == 5)
                    {
                        return "###,##0.00000";
                    }
                    if (ai_dp == 6)
                    {
                        return "###,##0.000000";
                    }
                    if (ai_dp == 7)
                    {
                        return "##,##0.0000000";
                    }
                    if (ai_dp == 8)
                    {
                        return "#,##0.00000000";
                    }
                }
                return "###,###,##0.00";
            }
            if (ai_dp == 0)
            {
                return "##,###,###,###";
            }
            if (ai_dp == 1)
            {
                return "###,###,###.0";
            }
            if (ai_dp != 2)
            {
                if (ai_dp == 3)
                {
                    return "##,###,###.000";
                }
                if (ai_dp == 4)
                {
                    return "#,###,###.0000";
                }
                if (ai_dp == 5)
                {
                    return "###,###.00000";
                }
                if (ai_dp == 6)
                {
                    return "###,###.000000";
                }
                if (ai_dp == 7)
                {
                    return "##,###.0000000";
                }
                if (ai_dp == 8)
                {
                    return "#,###.00000000";
                }
            }
            return "###,###,###.00";
        }
        public decimal ToSettingDecimal(string as_coID, string as_module, string as_column, decimal ad_decimal)
        {
            string text1;
            if (ad_decimal.ToString() == "")
            {
                ad_decimal = new decimal(0);
                return ad_decimal;
            }
            ad_decimal += new decimal(1, 0, 0, false, 10);
            if ((text1 = as_module) != null)
            {
                text1 = string.IsInterned(text1);
                if (text1 != "POP")
                {
                    if (text1 == "SOP")
                    {
                        if (as_column == "Q")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPreSopQty());
                        }
                        if (as_column == "P")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPreSopPrice());
                        }
                        if (as_column == "A")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPreSopAmt());
                        }
                        return ad_decimal;
                    }
                    if (text1 == "PSP")
                    {
                        if (as_column == "Q")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPrePspQty());
                        }
                        if (as_column == "P")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPrePspPrice());
                        }
                        if (as_column == "A")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPrePspAmt());
                        }
                        return ad_decimal;
                    }
                    if (text1 == "STK")
                    {
                        if (as_column == "Q")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPreStkQty());
                        }
                        if (as_column == "P")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPreStkPrice());
                        }
                        if (as_column == "A")
                        {
                            ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPreStkAmt());
                        }
                        return ad_decimal;
                    }
                }
                else
                {
                    if (as_column == "Q")
                    {
                        ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPrePopQty());
                    }
                    if (as_column == "P")
                    {
                        ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPrePopPrice());
                    }
                    if (as_column == "A")
                    {
                        ad_decimal = decimal.Round(ad_decimal, 2);//gVar.GetPrePopAmt());
                    }
                    return ad_decimal;
                }
            }

            ad_decimal = decimal.Round(ad_decimal, 2);
            return ad_decimal;
        }
        /// <summary>
        /// Round up to desired value with specified value.
        /// </summary>
        /// <param name="sender">Value to round up.</param>
        /// <param name="interval">Interval to round up. (e.g. 2,5,3..)</param>
        /// <returns></returns>
        /// <remarks>
        /// Can handle positive and negative integer only.
        /// 
        /// round up with 5
        /// 6 -> 5
        /// 8 -> 10
        /// 13 -> 10
        /// 16 -> 15
        /// 
        /// round up with 7
        /// 7 -> 7
        /// 9 -> 7
        /// 14 -> 14
        /// 12 -> 14
        /// </remarks>
        public static int RoundUp(int sender, int interval)
        {
            int output = 0;

            try
            {
                int x = 0;
                int r = 0;
                r = sender % interval;
                x = (sender - r) / interval;
                if (Math.Abs(r) > Math.Abs(interval / 2))
                    x = (r > 0) ? x + 1 : x - 1;
                output = interval * x;
                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }
        }
    }//end class Round

}//end namespace