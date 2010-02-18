using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Configuration;
using Plexus.Utils;
using Plexus.Utils.ExcelDataReader;

namespace Plexus.ERP
{
    public class ExcelDiff
    {
        #region Properties
        public bool IgnoreFirstRow { get; set; }
        private IDictionary<string, string> dictionary;

        public ExcelFile File1 { get; set; }
        public ExcelFile File2 { get; set; }
        private DataSet resultSet1;
        public DataSet ResultSet1 { get { return this.resultSet1; } }
        private DataSet resultSet2;
        public DataSet ResultSet2 { get { return this.resultSet2; } }
        public object Result { get; set; }
        //private DataTable table;
        //public DataTable Table { get { return this.table; } }
        #endregion

        public ExcelDiff()
        {
        }

        #region Methods
        public void Read()
        {
            Read(false);
        }
        public void Read(bool ignoreFirstRow)
        {
            this.IgnoreFirstRow = true;
            for (int i = 0; i < 2; i++)
            {
                switch (i)
                {
                    case 0:
                        if (this.File1 == null || this.File1.FileName == null) continue;
                        Process(this.File1, ref this.resultSet1);
                        break;
                    case 1:
                        if (this.File2 == null || this.File2.FileName == null) continue;
                        Process(this.File2, ref this.resultSet2);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Process from excel to dataset and list
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dataSet"></param>
        /// <param name="target"></param>
        /// <seealso>http://stackoverflow.com/questions/208532/how-do-you-convert-a-datatable-into-a-generic-list</seealso>
        private void Process(ExcelFile file, ref DataSet dataSet) //, ref List<Bom> target)
        {
            System.Diagnostics.Debug.WriteLine("Start parse excel at " + DateTime.Now);
            FileStream fs = new FileStream(file.FileName, FileMode.Open, FileAccess.Read);
            ExcelDataReader reader = new ExcelDataReader(fs);
            fs.Close();
            dataSet = reader.WorkbookData;
            System.Diagnostics.Debug.WriteLine("End parse at " + DateTime.Now);
            if (dataSet.Tables.Count > 0)
            {
                GetFileHeaders(dataSet.Tables[0], file);
                System.Diagnostics.Debug.WriteLine("Row count: " + dataSet.Tables[0].Rows.Count);
                if (this.IgnoreFirstRow) dataSet.Tables[0].Rows.RemoveAt(0);
            }

            List<DataRow> list = new List<DataRow>(dataSet.Tables[0].Select());
            if (this.IgnoreFirstRow) list.RemoveAt(0);

            ////todo: convert to BOM IEnumerable
            //target = new List<Bom>(
            //    from f in list
            //    select new Bom()
            //    {
            //        PartNumber = f[0].ToString(),
            //        Manufacturer = f[1].ToString(),
            //        ManufacturerPartNumber = f[2].ToString(),
            //        ManufacturerPreferredStatus = f[3].ToString(),
            //        RoHS = f[4].ToString(),
            //    }
            //    );
        }
        /// <summary>
        /// TODO: Differentiate two file
        /// </summary>
        public void Diff(ExcelDiffRule rule)
        {
            if (this.resultSet1 == null || this.resultSet2 == null) return;
            if (this.resultSet1.Tables.Count == 0 || this.resultSet2.Tables.Count == 0) return;

            //get dictionary for mapping
            GetDictionary(rule.MapTo);

            //add indicator show location of source
            DataColumn columnFirst = new DataColumn("Column" + Convert.ToString(this.resultSet1.Tables[0].Columns.Count + 1), typeof(string), "1");
            this.resultSet1.Tables[0].Columns.Add(columnFirst);
            List<DataRow> list1 = new List<DataRow>(this.resultSet1.Tables[0].Select());
            IEnumerable<DiffView> diff1 = null;
            Select(list1, rule, ref diff1);

            DataColumn columnSecond = new DataColumn("Column" + Convert.ToString(this.resultSet2.Tables[0].Columns.Count + 1), typeof(string), "2");
            this.resultSet2.Tables[0].Columns.Add(columnSecond);
            List<DataRow> list2 = new List<DataRow>(this.resultSet2.Tables[0].Select());
            IEnumerable<DiffView> diff2 = null;
            Select(list2, rule, ref diff2);

            IEnumerable<DiffView> merge = diff1.Union(diff2);
            System.Diagnostics.Debug.WriteLine("before: " + merge.Count());
            TrimEmpty(rule, ref merge);

            switch (rule.Keys.Length)
            {
                case 1:
                    merge = (rule.Type == ExcelDiffType.SimpleFuzzy) ?
                        merge.Distinct((new OneKeyFuzzyComparer())).OrderBy(f => f.Column1).ThenBy(f => f.Column2) :
                        merge.Distinct((new OneKeyComparer())).OrderBy(f => f.Column1).ThenBy(f => f.Column2);
                    break;
                case 2:
                    if (rule.Type == ExcelDiffType.SimpleFuzzy) merge = merge.Distinct((new OneKeyFuzzyComparer())).OrderBy(f => f.Column1).ThenBy(f => f.Column2);
                    merge = merge.Distinct((new TwoKeysComparer())).OrderBy(f => f.Column1).ThenBy(f => f.Column2);
                    break;
                case 3:
                    merge = merge.Distinct((new ThreeKeysComparer())).OrderBy(f => f.Column1).ThenBy(f => f.Column2);
                    break;
                case 4:
                    break;
            }
            this.Result = merge;
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="file"></param>
        private void GetFileHeaders(DataTable sender, ExcelFile file)
        {
            file.Headers = new List<string>();
            for (int i = 0; i < sender.Columns.Count; i++)
                file.Headers.Add(sender.Rows[0][i].ToString());
        }
        /// <summary>
        /// Get dictionary for mapping use
        /// </summary>
        /// <param name="fileName"></param>
        /// <remarks>Get once only for each time diff</remarks>
        private void GetDictionary(string fileName)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = fileName;
            Configuration conf = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            dictionary = new Dictionary<string, string>();
            foreach (string s in conf.AppSettings.Settings.AllKeys)
                dictionary.Add(new KeyValuePair<string, string>(s, conf.AppSettings.Settings[s].Value));
            //keys.Add(new KeyValuePair<string, string>("YAGEO", "YAGEO CORPORATION - MC62"));
            //keys.Add(new KeyValuePair<string, string>("VISHAY INTERTECHNOLOGY INC", "VISHAY ELECTRONIC GMBH - JF61"));
        }
        /// <summary>
        /// HACK: Trim empty
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="sender"></param>
        private void TrimEmpty(ExcelDiffRule rule, ref IEnumerable<DiffView> sender)
        {
            for (int i = 0; i < rule.Keys.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        sender = sender.Where(f => f.Column1 != null);
                        sender = sender.Where(f => f.Column1.Length > 0);
                        break;
                    case 1:
                        sender = sender.Where(f => f.Column2 != null);
                        sender = sender.Where(f => f.Column2.Length > 0);
                        break;
                    case 2:
                        sender = sender.Where(f => f.Column3 != null);
                        sender = sender.Where(f => f.Column3.Length > 0);
                        break;
                    case 3:
                        sender = sender.Where(f => f.Column4 != null);
                        sender = sender.Where(f => f.Column4.Length > 0);
                        break;
                    case 4:
                        sender = sender.Where(f => f.Column5 != null);
                        sender = sender.Where(f => f.Column5.Length > 0);
                        break;
                    default:
                        break;
                }
            }
        }
        private void Select(List<DataRow> sender, ExcelDiffRule rule, ref IEnumerable<DiffView> target)
        {
            switch (rule.Keys.Length)
            {
                case 1:
                    target = from f in sender
                             select new DiffView()
                             {
                                 Column1 = f[rule.Keys[0]].ToString(),
                                 Column2 = (rule.Type == ExcelDiffType.SimpleMap) ? Map(rule.MapTo, f[rule.Lookup].ToString()) : f[rule.Lookup].ToString(),
                                 Column3 = f[sender[0].Table.Columns.Count - 1].ToString(),
                             };
                    break;
                case 2:
                    target = from f in sender
                             select new DiffView()
                             {
                                 Column1 = f[rule.Keys[0]].ToString(),
                                 Column2 = f[rule.Keys[1]].ToString(),
                                 Column3 = (rule.Type == ExcelDiffType.SimpleMap) ? Map(rule.MapTo, f[rule.Lookup].ToString()) : f[rule.Lookup].ToString(),
                                 Column4 = f[sender[0].Table.Columns.Count - 1].ToString(),
                             };
                    break;
                case 3:
                    target = from f in sender
                             select new DiffView()
                             {
                                 Column1 = f[rule.Keys[0]].ToString(),
                                 Column2 = f[rule.Keys[1]].ToString(),
                                 Column3 = f[rule.Keys[2]].ToString(),
                                 Column4 = (rule.Type == ExcelDiffType.SimpleMap) ? Map(rule.MapTo, f[rule.Lookup].ToString()) : f[rule.Lookup].ToString(),
                                 Column5 = f[sender[0].Table.Columns.Count - 1].ToString(),
                             };
                    break;
                case 4:
                    target = from f in sender
                             select new DiffView()
                             {
                                 Column1 = f[rule.Keys[0]].ToString(),
                                 Column2 = f[rule.Keys[1]].ToString(),
                                 Column3 = f[rule.Keys[2]].ToString(),
                                 Column4 = f[rule.Keys[3]].ToString(),
                                 Column5 = (rule.Type == ExcelDiffType.SimpleMap) ? Map(rule.MapTo, f[rule.Lookup].ToString()) : f[rule.Lookup].ToString(),
                                 Column6 = f[sender[0].Table.Columns.Count - 1].ToString(),
                             };
                    break;
                case 5:
                    target = from f in sender
                             select new DiffView()
                             {
                                 Column1 = f[rule.Keys[0]].ToString(),
                                 Column2 = f[rule.Keys[1]].ToString(),
                                 Column3 = f[rule.Keys[2]].ToString(),
                                 Column4 = f[rule.Keys[3]].ToString(),
                                 Column5 = f[rule.Keys[4]].ToString(),
                                 Column6 = (rule.Type == ExcelDiffType.SimpleMap) ? Map(rule.MapTo, f[rule.Lookup].ToString()) : f[rule.Lookup].ToString(),
                                 Column7 = f[sender[0].Table.Columns.Count - 1].ToString(),
                             };
                    break;
                //todo for default
            }
        }
        /// <summary>
        /// Map to a common value. The key must be unique.
        /// </summary>
        /// <param name="fileName">File name to read</param>
        /// <param name="sender"></param>
        /// <seealso>http://msdn.microsoft.com/en-us/library/s4ys34ea.aspx</seealso>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Keep in a .xml or config file
        /// </remarks>
        private string Map(string fileName, string sender)
        {
            foreach (KeyValuePair<string, string> key in dictionary)
                if (sender == key.Key || sender == key.Value) return key.Key;
            return sender;
        }
        #endregion
    }
    /// <summary>
    /// Compare one key
    /// </summary>
    public class OneKeyComparer : IEqualityComparer<DiffView>
    {
        #region IEqualityComparer<DiffView> Members
        bool IEqualityComparer<DiffView>.Equals(DiffView x, DiffView y)
        {
            return (x.Column1 == y.Column1);
        }
        int IEqualityComparer<DiffView>.GetHashCode(DiffView obj)
        {
            return obj.Column1.GetHashCode();
        }
        #endregion
    }
    public class TwoKeysComparer : IEqualityComparer<DiffView>
    {

        #region IEqualityComparer<DiffView> Members
        bool IEqualityComparer<DiffView>.Equals(DiffView x, DiffView y)
        {
            return (x.Column1 == y.Column1 & x.Column2 == y.Column2);
        }
        int IEqualityComparer<DiffView>.GetHashCode(DiffView obj)
        {
            return obj.Column1.GetHashCode() * obj.Column2.GetHashCode();
        }
        #endregion
    }
    public class ThreeKeysComparer : IEqualityComparer<DiffView>
    {

        #region IEqualityComparer<DiffView> Members
        bool IEqualityComparer<DiffView>.Equals(DiffView x, DiffView y)
        {
            return (x.Column1 == y.Column1 & x.Column2 == y.Column2 & x.Column3 == y.Column3);
        }
        int IEqualityComparer<DiffView>.GetHashCode(DiffView obj)
        {
            return obj.Column1.GetHashCode() * obj.Column2.GetHashCode() * obj.Column3.GetHashCode();
        }
        #endregion
    }
    public class OneKeyFuzzyComparer : IEqualityComparer<DiffView>
    {
        #region IEqualityComparer<DiffView> Members
        bool IEqualityComparer<DiffView>.Equals(DiffView x, DiffView y)
        {
            string[] splits1 = x.Column1.Split(new char[] { ' ' });
            return y.Column1.Contains(splits1[0]);
        }
        int IEqualityComparer<DiffView>.GetHashCode(DiffView obj)
        {
            return obj.Column1.GetHashCode();
        }
        #endregion
    }
    /// <summary>
    /// Differentiate Result view
    /// </summary>
    /// <remarks>
    /// HACK: Find a better way to keep track unknow column size
    /// </remarks>
    public class DiffView
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
        public string Column6 { get; set; }
        public string Column7 { get; set; }
        public string Column8 { get; set; }
        public string Column9 { get; set; }
        public string Column10 { get; set; }
        public DiffView()
        {
        }
    }

    public enum ExcelDiffType
    {
        Simple,
        SimpleFuzzy,
        SimpleMap,
        Sum,
        Delimit,
    }
    public class ExcelRule : ExcelDiffRule
    {
        public ExcelRule(int[] keys, int lookup, string mapToPath)
        {
            this.Keys = keys;
            this.Lookup = lookup;
            this.MapTo = mapToPath;
        }
    }

    /// <summary>
    /// Excel Differentiate config rule setting
    /// </summary>
    /// <seealso>http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx</seealso>
    public class ExcelDiffRule : ConfigurationElement
    {
        #region Properties
        private static ConfigurationProperty id;
        public int Id { get { return (int)base[id]; } set { base[id] = value; } }
        private static ConfigurationProperty keys;
        public int[] Keys
        {
            //get { return (int[])base[keys]; }
            get
            {
                string[] splits = base[keys].ToString().Split(new char[] { ',' });
                int[] output = new int[splits.Length];
                for (int i = 0; i < output.Length; i++)
                    output[i] = Convert.ToInt32(splits[i]);
                return output;
            }
            //set { base[keys] = value; }
            set
            {
                string merged = string.Empty;
                int[] hold = (value as int[]);
                foreach (int i in hold)
                    merged += i + ",";
                merged = merged.TrimEnd(new char[] { ',' });
                base[keys] = merged;
            }
        }
        private static ConfigurationProperty lookup;
        public int Lookup { get { return (int)base[lookup]; } set { base[lookup] = value; } }
        private static ConfigurationProperty type;
        public ExcelDiffType Type { get { return (ExcelDiffType)base[type]; } set { base[type] = value; } }
        private static ConfigurationProperty mapto;
        public string MapTo { get { return (string)base[mapto]; } set { base[mapto] = value; } }
        private static ConfigurationPropertyCollection properties;
        protected override ConfigurationPropertyCollection Properties { get { return properties; } }
        #endregion

        static ExcelDiffRule()
        {
            id = new ConfigurationProperty("id", typeof(int), null, ConfigurationPropertyOptions.IsRequired);
            keys = new ConfigurationProperty("keys", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            lookup = new ConfigurationProperty("lookup", typeof(int), null, ConfigurationPropertyOptions.IsRequired);
            mapto = new ConfigurationProperty("mapto", typeof(string), null, ConfigurationPropertyOptions.None);
            type = new ConfigurationProperty("type", typeof(ExcelDiffType), null, ConfigurationPropertyOptions.None);
            properties = new ConfigurationPropertyCollection();
            properties.Add(id);
            properties.Add(keys);
            properties.Add(lookup);
            properties.Add(mapto);
            properties.Add(type);
        }
    }
    public class ExcelDiffRuleCollection : ConfigurationElementCollection
    {
        public ExcelDiffRuleCollection()
        {
        }
        public ExcelDiffRule this[int index]
        {
            get { return (ExcelDiffRule)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null) base.BaseRemoveAt(index);
                base.BaseAdd(index, value);
            }
        }
        public ExcelDiffRule this[string name] { get { return (ExcelDiffRule)base.BaseGet(name); } }

        #region Properties
        public override ConfigurationElementCollectionType CollectionType { get { return ConfigurationElementCollectionType.BasicMap; } }
        protected override string ElementName { get { return "excelRule"; } }
        protected override ConfigurationPropertyCollection Properties { get { return new ConfigurationPropertyCollection(); } }
        #endregion

        #region Methods
        public void Add(ExcelDiffRule item)
        {
            base.BaseAdd(item);
        }
        public void Remove(ExcelDiffRule item)
        {
            base.BaseRemove(item);
        }
        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExcelDiffRule();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ExcelDiffRule).Id;
        }
        #endregion
    }
    public class ExcelDiffRuleSection : ConfigurationSection
    {
        #region Properties
        private static ConfigurationPropertyCollection properties;
        protected override ConfigurationPropertyCollection Properties { get { return properties; } }
        private static ConfigurationProperty excelDiffRuleColection;
        public ExcelDiffRuleCollection Rules { get { return (ExcelDiffRuleCollection)base[excelDiffRuleColection]; } }
        #endregion
        static ExcelDiffRuleSection()
        {
            excelDiffRuleColection = new ConfigurationProperty(string.Empty, typeof(ExcelDiffRuleCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);//hack: must put or
            properties = new ConfigurationPropertyCollection();
            properties.Add(excelDiffRuleColection);
        }
    }

    public class ExcelFile
    {
        public string FileName { get; set; }
        public List<string> Headers { get; set; }
        public ExcelFile(string fileName)
        {
            this.FileName = fileName;
        }
    }
    public class ExcelRuleCreator
    {
        public List<ExcelDiffRule> Rules { get; set; }
        public ExcelRuleCreator()
        {
            this.Rules = new List<ExcelDiffRule>();
        }
    }


    /// <summary>
    /// TODO: Wrap the dictionary value for 2 files so it not appear in exception(diff) result
    /// </summary>
    public class Mapper
    {
        /// <summary>
        /// Section name to lookup. ie. Customer maintenance, or Status maintenance
        /// </summary>
        public string Section { get; set; }
        public Mapper()
        {
        }
    }
}