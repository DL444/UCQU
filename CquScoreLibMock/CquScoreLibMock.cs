// This is a dummy file for building and testing the app. 
// Most of the methods in Watcher class will not function as intended. 


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CquScoreLib
{
    public class ScoreSet : IEnumerable<Term>
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int AdmissionYear { get; private set; }
        public string Major { get; private set; }
        public string ManagementClass { get; private set; }
        public double GPA { get; private set; }
        public bool IsMajor { get; private set; }

        List<Term> terms = new List<Term>();

        public ScoreSet(string id, string name, int admissionYear, string major, string manageClass, bool isMajor)
        {
            Id = id;
            Name = name;
            AdmissionYear = admissionYear;
            Major = major;
            ManagementClass = manageClass;
            IsMajor = isMajor;
        }

        public double GetGPA()
        {
            double sumGP = 0;
            double sumCredit = 0;
            foreach (Term t in this)
            {
                foreach (Course c in t)
                {
                    double coursePA = c.GPA;
                    if (coursePA == 0.0) { continue; }
                    sumGP += c.Credit * coursePA;
                    sumCredit += c.Credit;
                }
            }
            if (sumCredit == 0) { GPA = 0; return GPA; }
            GPA = sumGP / sumCredit;
            return GPA;
        }
        public double GetGPA(int beginningYear)
        {
            double sumGP = 0;
            double sumCredit = 0;
            foreach (Term t in this)
            {
                if (t.BeginningYear != beginningYear) { continue; }
                foreach (Course c in t)
                {
                    double coursePA = c.GPA;
                    if (coursePA == 0.0) { continue; }
                    sumGP += c.Credit * coursePA;
                    sumCredit += c.Credit;
                }
            }
            if (sumCredit == 0) { return 0; }
            return sumGP / sumCredit;
        }

        public Term this[int index]
        {
            get => terms[index];
        }

        public void AddTerm(Term term)
        {
            terms.Add(term);
        }

        public IEnumerator<Term> GetEnumerator()
        {
            return terms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $"{Id} {Name} " + (IsMajor ? "主修" : "辅修");
        }
    }
    public class Term : IEnumerable<Course>
    {
        public int BeginningYear { get; private set; }
        public int EndingYear { get; private set; }
        public int TermNumber { get; private set; }
        public double GPA { get; private set; }

        public List<Course> CourseCollection { get; } = new List<Course>();

        public Term(int beginYear, int termNumber)
        {
            BeginningYear = beginYear;
            EndingYear = BeginningYear + 1;
            TermNumber = termNumber;
        }

        public double GetGPA()
        {
            double sumGP = 0;
            double sumCredit = 0;
            foreach (Course c in this)
            {
                double coursePA = c.GPA;
                if (coursePA == 0.0) { continue; }
                sumGP += c.Credit * coursePA;
                sumCredit += c.Credit;
            }
            if (sumCredit == 0) { GPA = 0; return GPA; }
            GPA = sumGP / sumCredit;
            return GPA;
        }

        public Course this[int index]
        {
            get => CourseCollection[index];
        }

        public void AddCourse(Course subject)
        {
            CourseCollection.Add(subject);
        }

        public IEnumerator<Course> GetEnumerator()
        {
            return CourseCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            string str = $"{BeginningYear}-{EndingYear}学年第";
            switch (TermNumber)
            {
                case 1: str += "一"; break;
                case 2: str += "二"; break;
                case 3: str += "三"; break;
                default: str += "零"; break;
            }
            return str + "学期";
        }
    }
    public class Course
    {
        public Course(string name, double credit, string category, bool isInitialTake, int score, bool isMajor, string comment, string lecturer, DateTime obtainedTime)
        {
            Name = name;
            Credit = credit;
            Category = category;
            IsInitialTake = isInitialTake;
            Score = score;
            IsMajor = isMajor;
            Comment = comment;
            Lecturer = lecturer;
            ObtainedTime = obtainedTime;
            GPA = GetGPA();
        }

        public string Name { get; private set; }
        public string SimplifiedName
        {
            get
            {
                String[] strs = Name.Split(']');
                if (strs.Length < 2) { return ""; }
                else { return strs[1]; }
            }
        }
        public double Credit { get; private set; }
        public string Category { get; private set; }
        public bool IsInitialTake { get; private set; }
        public int Score { get; private set; }

        public bool IsMajor { get; private set; }
        public string Comment { get; private set; }
        public string Lecturer { get; private set; }
        public string SimplifiedLecturer
        {
            get
            {
                String[] strs = Lecturer.Split(']');
                if (strs.Length < 2) { return ""; }
                else { return strs[1]; }
            }
        }
        public DateTime ObtainedTime { get; private set; }
        public double GPA { get; private set; }

        public double GetGPA()
        {
            if (Comment == "补考" || IsInitialTake == false || Comment == "缺考" || Comment == "补考(缺考)")
            {
                if (Score >= 60) { GPA = 1.0; return 1.0; }
                else { GPA = 0.0; return 0.0; }
            }
            if (Score > 90) { GPA = 4.0; return 4.0; }
            if (Score < 60) { GPA = 0.0; return 0.0; }
            GPA = 1.0 + 0.1 * (Score - 60);
            return GPA;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Watcher
    {
        private string passwordHash;
        private int retryCount = 5;

        public string Host { get; private set; }
        public string SessionId { get; private set; }
        public string UserName { get; private set; }
        public int PerformDelay { get; set; }
        public IEnumerable<string> Workload { get; set; }

        public Schedule Schedule { get; private set; }

        public Dictionary<string, ScoreSet> Result { get; private set; } = new Dictionary<string, ScoreSet>();


        public Watcher(string host, string userName, string passwordHash) : this(host, userName, passwordHash, new SingleWorkload("00000000")) { }
        public Watcher(string host, string userName, string passwordHash, IEnumerable<string> workload) : this(host, userName, passwordHash, workload, 500) { }
        public Watcher(string host, string userName, string passwordHash, int performDelay) : this(host, userName, passwordHash, new SingleWorkload("00000000"), performDelay) { }
        public Watcher(string host, string userName, string passwordHash, IEnumerable<string> workload, int performDelay) : this(host, userName, passwordHash, workload, performDelay, 5) { }
        public Watcher(string host, string userName, string passwordHash, IEnumerable<string> workload, int performDelay, int retryCount)
        {
            Host = host;
            this.Host = this.Host.Replace("http://", "");
            if (this.Host.EndsWith("/")) { this.Host = this.Host.Remove(this.Host.Length - 1); }
            UserName = userName; this.passwordHash = passwordHash;
            Workload = workload;
            PerformDelay = performDelay;
            SessionId = "";
            this.retryCount = retryCount;
        }

        public ScoreSet GetSet(String key)
        {
            ScoreSet set = null;
            if (Result.TryGetValue(key, out set))
            {
                return set;
            }
            else
            {
                return null;
            }
        }

        public void Reset()
        {
            Result.Clear();
            (Workload as IEnumerator).Reset();
        }

        public async Task<string> LoginAsync()
        {
            StringBuilder sessionIdBuilder = new StringBuilder();
            Random randomizer = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 24; i++)
            {
                char c = (char)randomizer.Next('a', 'z' + 1);
                if (randomizer.Next(0, 2) == 0)
                {
                    sessionIdBuilder.Append(c);
                }
                else
                {
                    sessionIdBuilder.Append(char.ToUpper(c));
                }
            }
            string sessionId = sessionIdBuilder.ToString();
            SessionId = sessionId;
            return sessionId;

        }
        public string Login()
        {
            return LoginAsync().GetAwaiter().GetResult();
        }
        async Task<HttpWebResponse> Scrap(string targetId, bool isMajor)
        {
            return null;
        }
        async Task<HttpWebResponse> Scrap(string targetId)
        {
            return await Scrap(targetId, true);
        }

        async Task<HttpWebResponse> ScrapSchedule(string term) 
        {
            return null;
        }
        public void WriteCsvStream(Stream stream, bool writeHeader, bool excelCompatible)
        {
            StringBuilder builder = new StringBuilder();
            ScoreSet[] sets = new ScoreSet[Result.Values.Count];
            Result.Values.CopyTo(sets, 0);
            if (writeHeader)
            {
                builder.Append($"\"学号\",");
                builder.Append($"\"姓名\",");
                builder.Append($"\"入学年份\",");
                builder.Append($"\"专业\",");
                builder.Append($"\"行政班级\",");
                builder.Append($"\"总GPA\",");
                builder.Append($"\"学期\",");
                builder.Append($"\"学期GPA\",");
                builder.Append($"\"课程名称\",");
                builder.Append($"\"课程学分\",");
                builder.Append($"\"课程类别\",");
                builder.Append("\"初修重修\",");
                builder.Append($"\"课程成绩\",");
                builder.Append("\"主修辅修\",");
                builder.Append($"\"备注\",");
                builder.Append($"\"任课教师\",");
                builder.Append($"\"成绩获得时间\",");
                builder.Append($"\"课程GPA\"");
            }
            foreach (ScoreSet s in sets)
            {
                foreach (Term t in s)
                {
                    foreach (Course c in t)
                    {
                        builder.AppendLine();
                        builder.Append($"\"{s.Id}\",");
                        builder.Append($"\"{s.Name}\",");
                        builder.Append($"\"{s.AdmissionYear}\",");
                        builder.Append($"\"{s.Major}\",");
                        builder.Append($"\"{s.ManagementClass}\",");
                        builder.Append($"\"{s.GPA}\",");
                        builder.Append($"\"{t.ToString()}\",");
                        builder.Append($"\"{t.GPA}\",");
                        builder.Append($"\"{c.Name}\",");
                        builder.Append($"\"{c.Credit}\",");
                        builder.Append($"\"{c.Category}\",");
                        builder.Append("\"" + (c.IsInitialTake ? "初修" : "重修") + "\",");
                        builder.Append($"\"{c.Score}\",");
                        builder.Append("\"" + (c.IsMajor ? "主修" : "辅修") + "\",");
                        builder.Append($"\"{c.Comment}\",");
                        builder.Append($"\"{c.Lecturer}\",");
                        builder.Append($"\"{c.ObtainedTime}\",");
                        builder.Append($"\"{c.GPA}\"");
                    }
                }
            }

            StreamWriter writer = new StreamWriter(stream, excelCompatible ? Encoding.Unicode : Encoding.GetEncoding(0));
            writer.BaseStream.Seek(0, SeekOrigin.End);
            writer.Write(builder.ToString());
            writer.Dispose();
        }
        public void WriteXmlStream(Stream stream)
        {
            ScoreSet[] sets = new ScoreSet[Result.Values.Count];
            Result.Values.CopyTo(sets, 0);

            XmlWriter writer = XmlWriter.Create(stream);
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("Sets");
            doc.AppendChild(root);
            foreach (ScoreSet s in sets)
            {
                XmlNode setNode = doc.CreateElement("Set");
                setNode.Attributes.Append(GenerateAttribute("IsMajor", s.IsMajor));
                setNode.Attributes.Append(GenerateAttribute("GrandGPA", $"{s.GPA:0.0}"));
                setNode.Attributes.Append(GenerateAttribute("ManagementClass", s.ManagementClass));
                setNode.Attributes.Append(GenerateAttribute("Major", s.Major));
                setNode.Attributes.Append(GenerateAttribute("AdmissionYear", s.AdmissionYear));
                setNode.Attributes.Append(GenerateAttribute("Name", s.Name));
                setNode.Attributes.Append(GenerateAttribute("Id", s.Id));

                foreach (Term t in s)
                {
                    XmlNode termNode = doc.CreateElement("Term");
                    termNode.Attributes.Append(GenerateAttribute("TermGPA", $"{t.GPA:0.0}"));
                    termNode.Attributes.Append(GenerateAttribute("TermNumber", t.TermNumber));
                    termNode.Attributes.Append(GenerateAttribute("EndingYear", t.EndingYear));
                    termNode.Attributes.Append(GenerateAttribute("BeginningYear", t.BeginningYear));

                    foreach (Course c in t)
                    {
                        XmlNode cNode = doc.CreateElement("Course");

                        cNode.Attributes.Append(GenerateAttribute("ObtainedTime", c.ObtainedTime));
                        cNode.Attributes.Append(GenerateAttribute("Lecturer", c.Lecturer));
                        cNode.Attributes.Append(GenerateAttribute("Comment", c.Comment));
                        cNode.Attributes.Append(GenerateAttribute("IsInitialTake", c.IsInitialTake));
                        cNode.Attributes.Append(GenerateAttribute("IsMajor", c.IsMajor));
                        cNode.Attributes.Append(GenerateAttribute("CourseGPA", $"{c.GPA:0.0}"));
                        cNode.Attributes.Append(GenerateAttribute("Score", c.Score));
                        cNode.Attributes.Append(GenerateAttribute("Category", c.Category));
                        cNode.Attributes.Append(GenerateAttribute("Credit", $"{c.Credit:0.0}"));
                        cNode.Attributes.Append(GenerateAttribute("Name", c.Name));

                        termNode.AppendChild(cNode);
                    }

                    setNode.AppendChild(termNode);
                }

                root.AppendChild(setNode);
            }

            doc.WriteTo(writer);
            writer.Dispose();

            XmlAttribute GenerateAttribute(string name, object value)
            {
                XmlAttribute attr = doc.CreateAttribute(name);
                attr.Value = value.ToString();
                return attr;
            }
        }
        public async Task<bool> ValidateLoginAsync()
        {
            return true;
        }
        public bool ValidateLogin()
        {
            return ValidateLoginAsync().GetAwaiter().GetResult();
        }
        public void RefreshLogin()
        {
            RefreshLogin(false);
        }
        public void RefreshLogin(bool force)
        {
            if (force == true || ValidateLogin() == false)
            {
                SessionId = Login();
            }
        }
        public async Task Perform()
        {
            foreach (string id in Workload)
            {
                bool major = true;
                HttpWebResponse webResponse = await Scrap(id, major);
                ScoreSet set = FormatPage(GetResponseString(webResponse), major);
                if (set == null)
                {
                    SetProcessed?.Invoke(this, new SetProcessedEventArgs(id, major, false));
                }
                else
                {
                    Result[id + "_0"] = set;
                    SetProcessed?.Invoke(this, new SetProcessedEventArgs(set.Id, set.IsMajor, true));
                }
                await Task.Delay(PerformDelay);

                major = false;
                webResponse = await Scrap(id, major);
                set = FormatPage(GetResponseString(webResponse), major);
                if (set == null)
                {
                    SetProcessed?.Invoke(this, new SetProcessedEventArgs(id, major, false));
                    continue;
                }
                Result[id + "_1"] = set;
                SetProcessed?.Invoke(this, new SetProcessedEventArgs(set.Id, set.IsMajor, true));
                await Task.Delay(PerformDelay);
            }
        }

        public async Task<Schedule> PerformSchedule(string term)
        {
            HttpWebResponse response = await ScrapSchedule(term);
            Schedule = FormatSchedule(GetResponseString(response));
            return Schedule;
        }

        static ScoreSet FormatPage(string response)
        {
            return FormatPage(response, true);
        }
        static ScoreSet FormatPage(string response, bool isMajor)
        {
            string setId = "00000000";
            string setName = "张三";
            string setYear = "2018";
            string setMajor = "某某专业";
            string setManageClass = "某某专业 00";

            ScoreSet set = new ScoreSet(setId, setName, int.Parse(setYear), setMajor, setManageClass, isMajor);
            List<Term> terms = GetTerms(response);

            for (int i = 0; i < terms.Count; i++)
            {
                    terms[i].AddCourse(new Course("[1]Course1", 1.0, "Category1", true,
                        85, true, "", "[1]Lecturer1", DateTime.Today));
                    terms[i].AddCourse(new Course("[2]Course2", 2.0, "Category2", true,
                        73, true, "", "[2]Lecturer2", DateTime.Today));
                    terms[i].AddCourse(new Course("[3]Course3", 1.0, "Category3", false,
                        65, true, "", "[3]Lecturer3", DateTime.Today));
                    terms[i].AddCourse(new Course("[4]Course4", 1.0, "Category4", true,
                        97, true, "", "[4]Lecturer4", DateTime.Today));
                    terms[i].AddCourse(new Course("[5]Course5", 1.0, "Category5", true,
                        44, true, "补考", "[5]Lecturer5", DateTime.Today));
                terms[i].GetGPA();
                set.AddTerm(terms[i]);
            }
            set.GetGPA();
            return set;

            List<Term> GetTerms(string input)
            {
                List<Term> termsCollection = new List<Term>();
                termsCollection.Add(new Term(2018, 1));
                termsCollection.Add(new Term(2018, 2));
                return termsCollection;
            }
        }

        Schedule FormatSchedule(string response)
        {
            Schedule schedule = new Schedule();
            schedule.AddEntry(1, new ScheduleEntry("Test1", "Lecturer1", 1, 1, 2, "Room1"));
            schedule.AddEntry(1, new ScheduleEntry("Test2", "Lecturer2", 1, 3, 4, "Room2"));
            schedule.AddEntry(1, new ScheduleEntry("Test3", "Lecturer3", 1, 5, 6, "Room3"));
            schedule.AddEntry(1, new ScheduleEntry("Test4", "Lecturer4", 2, 3, 4, "Room4"));
            schedule.AddEntry(1, new ScheduleEntry("Test5", "Lecturer5", 3, 5, 8, "Room5"));
            schedule.AddEntry(2, new ScheduleEntry("Test6", "Lecturer6", 6, 1, 3, "Room6"));
            schedule.AddEntry(2, new ScheduleEntry("Test7", "Lecturer7", 7, 5, 6, "Room7"));
            schedule.AddEntry(4, new ScheduleEntry("Test8", "Lecturer8", 3, 3, 4, "Room8"));
            return schedule;
        }

        public static string GetPasswordHash(string userId, string pwd)
        {
            return "";
        }
        static string GetResponseString(HttpWebResponse response)
        {
            if (response == null) { return null; }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding gb2312Encoder = Encoding.GetEncoding("GB2312");
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, gb2312Encoder);
            string responseString = reader.ReadToEnd();
            reader.Dispose();
            return responseString;
        }

        public List<string> GetStudentId(string identifier)
        {
            return GetStudentIdAsync(identifier).GetAwaiter().GetResult();
        }
        public async Task<List<string>> GetStudentIdAsync(string identifier)
        {
            return new List<string>() { "00000000", "00000001" };
        }

        public event EventHandler<SetProcessedEventArgs> SetProcessed;
        public class SetProcessedEventArgs : EventArgs
        {
            public SetProcessedEventArgs(string id, bool isMajor, bool isValid)
            {
                Id = id;
                IsMajor = isMajor;
                IsValid = isValid;
            }

            public string Id { get; private set; }
            public bool IsMajor { get; private set; }
            public bool IsValid { get; private set; }
        }
    }

    public class SequentialWorkload : IEnumerable<string>, IEnumerator<string>
    {
        public SequentialWorkload(int startIndex, int count)
        {
            this.StartIndex = startIndex;
            current = StartIndex - 1;
            Count = count;
        }

        public int StartIndex { get; set; }
        public int Count { get; set; }

        int current = 0;

        public string Current => current.ToString();

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            current++;
            if (current < StartIndex + Count) { return true; }
            return false;
        }

        public void Reset()
        {
            current = StartIndex - 1;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class SingleWorkload : IEnumerator<string>, IEnumerable<string>
    {
        bool requested = false;
        public SingleWorkload(string workload) => Workload = workload;

        public string Workload { get; set; }

        public string Current
        {
            get
            {
                requested = true;
                return Workload;
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            return !requested;
        }

        public void Reset() { requested = false; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class StringListWorkload : IEnumerator<string>, IEnumerable<string>
    {
        public List<string> WorkloadList { get; set; }
        int currentIndex = -1;

        public StringListWorkload(List<string> list)
        {
            WorkloadList = list ?? throw new ArgumentNullException(nameof(list));
        }

        public string Current => WorkloadList[currentIndex];
        public int Count => WorkloadList.Count;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            currentIndex++;
            if (currentIndex < WorkloadList.Count) { return true; }
            return false;
        }

        public void Reset()
        {
            currentIndex = -1;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Schedule
    {
        Dictionary<string, List<ScheduleEntry>> schedule = new Dictionary<string, List<ScheduleEntry>>();

        public List<ScheduleEntry> this[int index]
        {
            get
            {
                if (schedule.ContainsKey(index.ToString()))
                {
                    return schedule[index.ToString()];
                }
                return new List<ScheduleEntry>();
            }
        }
        public List<ScheduleEntry> this[string index] => schedule[index];
        public int Count => schedule.Keys.Count;

        public void AddEntry(int week, ScheduleEntry entry)
        {
            if (!schedule.ContainsKey(week.ToString()))
            {
                for (int i = 1; i <= week; i++)
                {
                    if (!schedule.ContainsKey(i.ToString())) { schedule[i.ToString()] = new List<ScheduleEntry>(); }
                }
            }
            schedule[week.ToString()].Add(entry);
        }

        public List<ScheduleEntry> GetDaySchedule(int day)
        {
            List<ScheduleEntry> entries = new List<ScheduleEntry>();
            int week = day / 7 + 1;
            int dayOfWeek = day % 7 + 1;
            if (week > Count || week < 1) { return entries; }
            entries = new List<ScheduleEntry>(this[week.ToString()].FindAll((ScheduleEntry x) => { return x.DayOfWeek == dayOfWeek; }));
            return entries;
        }
    }
    public class ScheduleEntry : IComparable<ScheduleEntry>
    {
        public ScheduleEntry(string name, string lecturer, int dayOfWeek, int startSlot, int endSlot, string room)
        {
            Name = name;
            Lecturer = lecturer;
            DayOfWeek = dayOfWeek;
            StartSlot = startSlot;
            EndSlot = endSlot;
            Room = room;
        }

        public string Name { get; private set; }
        public string Lecturer { get; private set; }
        public int DayOfWeek { get; private set; }
        public int StartSlot { get; private set; }
        public int EndSlot { get; private set; }
        public string Room { get; private set; }

        public int CompareTo(ScheduleEntry other)
        {
            int dayComp = DayOfWeek.CompareTo(other.DayOfWeek);
            if (dayComp == 0)
            {
                return StartSlot.CompareTo(other.StartSlot);
            }
            return dayComp;
        }

        public string SessionSpan
        {
            get
            {
                if (StartSlot == EndSlot)
                {
                    return $"{StartSlot}";
                }
                else
                {
                    return $"{StartSlot}-{EndSlot}";
                }
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
