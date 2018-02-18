using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CrusaderKingsStoryGen
{
    class ScriptReference
    {
        public String Referenced { get; set; }

        public ScriptReference(String r)
        {
            Referenced = r;
        }

        public override string ToString()
        {
            return Referenced;
        }
    }
    class ScriptCommand
    {
        public ScriptScope Parent;
        public string Name { get; set; }
        public object Value { get; set; }
        public object Replaced { get; set; }
        public bool AlwaysQuote { get; set; }
        public string Op { get; set; }

        public ScriptCommand()
        {
         
        }
        public ScriptCommand(string name, object val, ScriptScope parent)
        {
            this.Parent = parent;
            Name = name;
            Value = val;
            if (val == null || val.ToString().Length == 0)
                Value = "\"\"";
        }
        public override string ToString()
        {
            return Name + " : " + Value;
        }

        public ScriptCommand Copy()
        {
            var s = new ScriptCommand(Name, Value, null);
            s.AlwaysQuote = AlwaysQuote;
            return s;
        }

        public int GetInt()
        {
            var v = Value as ScriptReference;

         
            if (v != null)
                return Convert.ToInt32(v.Referenced);

            if (Value.ToString() == "-")
                return 0;

            if (Value is String)
                return Convert.ToInt32(Value);

            return (int) Value;
        }
    }

    class ScriptScope
    {
        public bool FromVanilla { get; set; } = false;
        public ScriptScope Parent { get; set; }
        public List<object> Children = new List<object>();
        public HashSet<object> ChildrenHas = new HashSet<object>();
        public List<ScriptScope> Scopes = new List<ScriptScope>();
        public string Name { get; set; }

        public object Tag1;
        public string Data
        {

            get { return _data; }
            set { _data = "\t\t\t" + value.Trim(); }
        }

        internal delegate void CopyDelegate(object o);

        public void FillFrom(ScriptScope from, CopyDelegate copy)
        {
            Name = from.Name;
            Parent = this.Parent;
            foreach (var child in from.Children)
            {
                if (child is ScriptScope)
                {
                    ScriptScope sc = new ScriptScope((child as ScriptScope).Name);
                    
                    sc.FillFrom(child as ScriptScope, copy);
                    copy.Invoke(child);
                    Add(sc);
                }
                else
                {
                    ScriptCommand c = new ScriptCommand();
                    ScriptCommand src = child as ScriptCommand;

                    c.Name = src.Name;
                    c.AlwaysQuote = src.AlwaysQuote;
                    c.Op = src.Op;
                    c.Parent = this;
                    copy.Invoke(c);
                    Add(c);
                }
            }
        }
        public override string ToString()
        {
            return Name;
        }

        public void Save(StreamWriter file, int depth)
        {
          
            String tab = "";
            for (int n = 1; n < depth; n++)
            {
                tab += "\t";
            }
            string ret = "\n";

            if (depth > 0)
                file.Write(tab + Name + " = {" + ret);

            string rett = "\t";
            tab = "";
            for (int n = 1; n < depth; n++)
            {
                tab += rett;
            }
            foreach (var child in Children)
            {
                if (child is ScriptCommand)
                {
                    ScriptCommand c = child as ScriptCommand;
                    if (c.AlwaysQuote)
                    {
                        String str = GetExportStringFromObject(c.Value);
                        if (!str.Trim().StartsWith("\""))
                            str = '"'.ToString() + str + '"'.ToString();

                        if (!string.IsNullOrEmpty(c.Op))
                        {
                            file.Write(tab + rett + c.Name + " " + c.Op + " " + str + ret);
                        }
                        else file.Write(tab + rett + c.Name + " = " + str + ret);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(c.Op))
                        {
                            file.Write(tab + rett + c.Name + " " + c.Op + " " + GetExportStringFromObject(c.Value) + ret);
                        }
                        else
                            file.Write(tab + rett + c.Name + " = " + GetExportStringFromObject(c.Value) + ret);
                    }
                }
                if (child is ScriptScope)
                {
                    (child as ScriptScope).Save(file, depth + 1);
                }
                if (child is String)
                {
                    file.WriteLine(child); //child as ScriptScope);.Save(file, depth + 1);
                }
            }
            if (Data.Trim().Length > 0)
                file.Write(Data);
            if (depth > 0)
            {
                if (depth == 1)
                    file.Write(tab + "}\n\n");
                else
                    file.Write(tab + "}\n");

            }

        }

        private string GetExportStringFromObject(object value)
        {
            
            if (value == null)
                return "";
            if (value is bool)
            {
                if (((bool) value) == true)
                    return "yes";
                else
                    return "no";
            }
            if (value is Color)
            {
                try
                {
                    Color col = (Color) value;
                    return "{ " + col.R + " " + col.G + " " + col.B + " }";

                }
                catch (Exception ex)
                {
                    System.Console.Out.WriteLine(ex.StackTrace);

                }
            }
            if (value is ScriptReference)
            {
                return ((ScriptReference) value).Referenced;
            }
            if (value is int || value is float)
            {
                return value.ToString();
            }
            if (value is List<String>)
            {
                return (value as List<String>).ToSpaceDelimited();
            }
            if (value.ToString().Contains(" "))
                return "\"" + value.ToString() + "\"";
            else
                return value.ToString();
        }

        public bool AllowDuplicates { get; set; }
        public string NameSearchReplaced { get; set; }

        public Dictionary<String, object> UnsavedData = new Dictionary<string, object>();

        public void SetChild(ScriptScope scope)
        {
            if (scope == null)
                return;


            if (!ChildrenHas.Contains(scope))
            {
                if (scope.Parent != null)
                {
                    scope.Parent.Remove(scope);
                }

                Add(scope);
                scope.Parent = this;
            }

        }

        public void SetChildPre(ScriptScope scope)
        {
            if (!Children.Contains(scope))
            {
                if (scope.Parent != null)
                {
                    scope.Parent.Remove(scope);
                }
                Insert(0, scope);
                scope.Parent = this;
            }

        }

        public bool HasNamed(string s)
        {
            foreach (var child in Children)
            {
                if (child is ScriptScope)
                    if (((ScriptScope) child).Name == s)
                        return true;
                if (child is ScriptCommand)
                    if (((ScriptCommand) child).Name == s)
                        return true;

            }

            return false;
        }

        public void Delete(string s)
        {
            var a = Children.ToArray();
            foreach (var child in a)
            {
                if (child is ScriptScope)
                    if (((ScriptScope) child).Name == s)
                        Remove(child);
                if (child is ScriptCommand)
                    if (((ScriptCommand) child).Name == s)
                        Remove(child);

            }
        }

        public void Add(string property, object value)
        {
            Add(new ScriptCommand() {Name = property, Value = value});
        }

        public ScriptScope AddScope(string name)
        {
            ScriptScope scope = new ScriptScope();
            scope.Name = name;
            scope.Parent = this;
            Add(scope);
            return scope;
        }

        public ScriptScope AddScope()
        {
            ScriptScope scope = new ScriptScope();
            scope.Parent = this;
            Add(scope);
            return scope;
        }

        public ScriptScope()
        {
            AllowDuplicates = true;
        }

        public ScriptScope(String name)
        {
            AllowDuplicates = true;
            this.Name = name;
        }

        public void Remove(object scriptScope)
        {
            ChildrenHas.Remove(scriptScope);
            Children.Remove(scriptScope);
            if (scriptScope is ScriptScope)
            {
                ChildrenMap.Remove(((ScriptScope) scriptScope).Name);
                Scopes.Remove(((ScriptScope) scriptScope));
                ((ScriptScope) scriptScope).Parent = null;
            }
            if (scriptScope is ScriptCommand)
            {
                ChildrenMap.Remove(((ScriptCommand) scriptScope).Name);
                ((ScriptCommand) scriptScope).Parent = null;
            }
        }

        public void Insert(int index, ScriptScope scriptScope)
        {
            if (!AllowDuplicates && ChildrenMap.ContainsKey(scriptScope.Name))
                return;

            ChildrenHas.Add(scriptScope);
            Children.Insert(index, scriptScope);
            if (scriptScope.Name == null)
                scriptScope.Name = ChildrenMap.Count.ToString();
            ChildrenMap[scriptScope.Name] = scriptScope;
            Scopes.Add(scriptScope);
            scriptScope.Parent = this;
        }

        public void Add(ScriptScope scriptScope)
        {
            if (!AllowDuplicates && ChildrenMap.ContainsKey(scriptScope.Name))
                return;
            ChildrenHas.Add(scriptScope);
            Children.Add(scriptScope);
            if (scriptScope.Name == null)
                scriptScope.Name = ChildrenMap.Count.ToString();
            ChildrenMap[scriptScope.Name] = scriptScope;
            Scopes.Add(scriptScope);
            scriptScope.Parent = this;
        }

        public void Add(ScriptCommand scriptScope)
        {
            if (!AllowDuplicates && ChildrenMap.ContainsKey(scriptScope.Name))
                return;

            ChildrenHas.Add(scriptScope);
            Children.Add(scriptScope);
            ChildrenMap[scriptScope.Name] = scriptScope;
            scriptScope.Parent = this;
        }

        public void Add(String str)
        {
            Children.Add(str);
            ChildrenMap[str] = str;
        }

        internal Dictionary<String, object> ChildrenMap = new Dictionary<string, object>();
        private string _data = "";

        public void Clear()
        {
            ChildrenHas.Clear();
            Children.Clear();
            ChildrenMap.Clear();
            Scopes.Clear();
        }

        public void RemoveAt(int property)
        {
            Remove(Children[property]);
        }

        public void Do(string line)
        {
            ScriptLoader.instance.LoadString(line, this);

        }

        public void Strip(string[] strings)
        {
            foreach (var s in strings)
            {
                if (this.ChildrenMap.ContainsKey(s))
                {
                    for (int index = 0; index < Children.Count; index++)
                    {
                        var child = Children[index];
                        if (child is ScriptScope)
                        {
                            if (((ScriptScope) child).Name == s)
                            {
                                Remove(child);
                            }
                        }
                        if (child is ScriptCommand)
                        {
                            if (((ScriptCommand) child).Name == s)
                            {
                                Remove(child);
                            }
                        }
                    }
                }
            }

        }


        public object Find(string name)
        {
            foreach (var child in Children)
            {
                if (child is ScriptCommand)
                    if (((ScriptCommand) child).Name == name)
                        return child;
                if (child is ScriptScope)
                    if (((ScriptScope) child).Name == name)
                        return child;
            }

            return null;
        }

        public void Command(string path)
        {
            String[] split = path.Trim().Split(' ');

            if (split[0] == "delete")
            {
                var o = PathTo(split[1]);
                Delete(o);
            }

            if (split[0] == "copy")
            {
                var o = PathTo(split[1]);
                var o2 = PathTo(split[2]);

                //      if()
            }

            if (split[0] == "overwrite")
            {
                var o = PathTo(split[1]);
                var o2 = PathTo(split[2]);

                if (o2 is ScriptScope)
                {
                    var dest = ((ScriptScope) o2);

                    dest.Clear();

                    if (o is ScriptScope)
                    {
                        var src = ((ScriptScope) o);

                        foreach (var child in src.Children)
                        {
                            dest.AddCopy(child);
                        }
                    }
                }
                //      if()
            }
        }

        public void AddCopy(object child)
        {
            if (child is ScriptScope)
            {
                var newScope = ((ScriptScope) child).Copy();
                Add(newScope);
            }
            else
            {
                var newparam = ((ScriptCommand) child).Copy();
                Add(newparam);

            }
        }

        private ScriptScope Copy()
        {
            ScriptScope newS = new ScriptScope();
            newS.Name = Name;
            foreach (var child in Children)
            {
                if (child is ScriptScope)
                {
                    newS.Add(((ScriptScope) child).Copy());
                }
                if (child is ScriptCommand)
                {
                    var c = ((ScriptCommand) child).Copy();

                    newS.Add(c);
                }
            }

            return newS;
        }

        private void Delete(object o)
        {
            if (o is ScriptScope)
                ((ScriptScope) o).Parent.Remove(o);
            if (o is ScriptCommand)
                ((ScriptCommand) o).Parent.Remove(o);
        }

        public object PathTo(string path)
        {
            String[] split = path.Trim().Split('.');
            int index = 0;
            if (split[0].Contains("["))
            {
                index = Convert.ToInt32(split[0].Split(new[] {'[', ']'})[1]);
                split[0] = split[0].Split(new[] {'[', ']'})[0];
            }
            if (ChildrenMap.ContainsKey(split[0]))
            {
                int found = 0;
                foreach (var child in Children)
                {
                    String name = GetName(child);
                    if (name == split[0])
                    {
                        if (found == index)
                        {
                            if (child is ScriptScope)
                            {
                                String s = "";
                                for (int x = 1; x < split.Length; x++)
                                    s += split[x] + ".";
                                if (s.Trim().Length == 0)
                                    return child;

                                s = s.Substring(0, s.Length - 1);

                                return ((ScriptScope) child).PathTo(s);
                            }
                            else
                            {
                                String s = "";
                                for (int x = 1; x < split.Length; x++)
                                    s += split[x] + ".";

                                s = s.Substring(0, s.Length - 1);

                                return ((ScriptCommand) child);
                            }

                        }
                        found++;
                    }
                }
            }

            return null;
        }

        private string GetName(object child)
        {
            if (child is ScriptScope)
                return ((ScriptScope) child).Name;
            if (child is ScriptCommand)
                return ((ScriptCommand) child).Name;

            return "";
        }

        public void WriteHierarchy(XmlWriter writer)
        {
            foreach (var rootChild in Children)
            {
                var scope = rootChild as ScriptScope;

                if (scope != null)
                {
             
                    writer.WriteStartElement(scope.Name);

                    scope.WriteHierarchy(writer);

                    writer.WriteEndElement();

                }

            }
        }

        public void SaveXml(XmlWriter writer)
        {
            writer.WriteStartElement("script");
            writer.WriteElementString("name", Name);
            foreach (var rootChild in Children)
            {
                var scope = rootChild as ScriptScope;

                if (scope != null)
                {

                    writer.WriteStartElement(scope.Name);

                    scope.SaveXml(writer);

                    writer.WriteEndElement();

                }

                var command = rootChild as ScriptCommand;

                if (command != null)
                {
                 
                    writer.WriteElementString(command.Name, command.Value.ToString());
                }

            }
            writer.WriteEndElement();
        }

        public bool TestBoolean(string name)
        {
            if (HasNamed(name))
            {
                foreach (var child in Children)
                {                    
                    if (child is ScriptCommand)
                        if (((ScriptCommand)child).Name == name)
                            return (bool)(((ScriptCommand)child).Value);
                }
            }

            return false;

        }

        public string GetString(string name)
        {
            if (HasNamed(name))
            {
                foreach (var child in Children)
                {
                    if (child is ScriptCommand)
                        if (((ScriptCommand)child).Name == name)
                            return (((ScriptCommand)child).Value).ToString();
                }
            }

            return "";
        }
        public object GetValue(string name)
        {
            if (HasNamed(name))
            {
                foreach (var child in Children)
                {
                    if (child is ScriptCommand)
                        if (((ScriptCommand)child).Name == name)
                            return (((ScriptCommand)child).Value);
                }
            }

            return "";
        }

        public int GetInt(string name)
        {
            if (HasNamed(name))
            {
                foreach (var child in Children)
                {
                    if (child is ScriptCommand)
                        if (((ScriptCommand) child).Name == name)
                        {
                            if (((ScriptCommand) child).Value is ScriptReference)
                            {
                                return Convert.ToInt32((((ScriptCommand) child).Value as ScriptReference).Referenced);
                            }
                            else
                            {
                                return Convert.ToInt32(((ScriptCommand)child).Value);
                            }
                        }
                }
            }


            return 0;
        }

        public Color GetColor(string name)
        {
            if (HasNamed(name))
            {
                foreach (var child in Children)
                {
                    if (child is ScriptCommand)
                        if (((ScriptCommand)child).Name == name)
                        {
                           
                            {
                                return (Color)(((ScriptCommand)child).Value);
                            }
                        }
                }
            }


            return Color.Black;
        }
    }

    // Above is a toolbox. it's just documented poorly.

    // Below is, presumably, a parser. It has its own data structure post-parse, but it's undocumented.

    class Script
    {
        public string Name { get; set; }
        public ScriptScope Root { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public void Save()
        {
  
            Directory.CreateDirectory(Globals.ModDir);
            Directory.CreateDirectory(Globals.ModDir + "common\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\cultures\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\governments\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\dynasties\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\disease\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\landed_titles\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\province_setup\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\religions\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\religious_titles\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\scripted_triggers\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\on_actions\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\societies\\");
            Directory.CreateDirectory(Globals.ModDir + "common\\trade_routes\\");                
            Directory.CreateDirectory(Globals.ModDir + "common\\traits\\");
            Directory.CreateDirectory(Globals.ModDir + "history\\");
            Directory.CreateDirectory(Globals.ModDir + "history\\characters\\");
            Directory.CreateDirectory(Globals.ModDir + "history\\provinces\\");
            Directory.CreateDirectory(Globals.ModDir + "history\\titles\\");
            Directory.CreateDirectory(Globals.ModDir + "history\\wars\\");
            Directory.CreateDirectory(Globals.ModDir + "decisions\\");
            Directory.CreateDirectory(Globals.ModDir + "events\\");
            Directory.CreateDirectory(Globals.ModDir + "gfx\\");
            Directory.CreateDirectory(Globals.ModDir + "gfx\\traits\\");
            Directory.CreateDirectory(Globals.ModDir + "gfx\\flags\\");
            Directory.CreateDirectory(Globals.ModDir + "interface\\");
            Directory.CreateDirectory(Globals.ModDir + "localisation\\");
            Directory.CreateDirectory(Globals.ModDir + "map\\");
          

            var filename = ConvertFileName(Name, filenameIsKey);
            try
            {
                using (System.IO.StreamWriter file =
              new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
                {


                    this.Root.Save(file, 0);

                    file.Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to save " + filename, "Error");
                throw;
            }
        }

        internal bool filenameIsKey = false;
        public static string ConvertFileName(String filename, bool isKey = false)
        {
            if (!isKey)
            {
                filename = filename.Replace(Globals.GameDir, "");
                filename = filename.Replace("storygen\\", Globals.ModName + "\\");
                filename = filename.Replace(Globals.MapDir, "");
                filename = filename.Replace(Directory.GetCurrentDirectory() + "\\data\\decisiontemplates\\", "");
                filename = filename.Replace(Globals.ModDir, "");
                filename = filename.Replace(Globals.OModDir, "");

            }
      
            filename = Globals.ModDir + filename;

            filename = filename.Replace("\\", "/");
            return filename;
        }
    }

    class ScriptLoader
    {
        public static ScriptLoader instance = new ScriptLoader();

        private int lineNum = 0;
        Stack<ScriptScope> scopeStack = new Stack<ScriptScope>();

        public List<String> TokenizeLine(String line)
        {
            if (line.Contains("b_arbil"))
            {
                
            }
            line = line.Split('#')[0];
            List<String> tokens = new List<string>();
            while (line.Trim().Length > 0)
            {
                if (line.Trim().StartsWith("#"))
                    return tokens;
                line = line.Trim();
                String token = line.GetNextToken(out line);
                line = line.Trim();
                tokens.Add(token);
            }

            return tokens;
        } 

        public ScriptScope LoadString(string line, ScriptScope target)
        {
            List<String> tokens = new List<string>();
       
            Script script =new Script();
            script.Root = target;
            String tokeniseLine = "";
            if (line.Contains("\n"))
            {
                var lines = line.Split('\n');
                current = target;
                tokeniseLine = "";
                foreach (var s in lines)
                {
                    if (s.Contains("society_rank = {"))
                    {

                    }
                    string ss = s.Split('#')[0];
                    tokeniseLine += ss.Trim();
                    if (tokeniseLine.Contains("#"))
                        tokeniseLine = tokeniseLine.Substring(0, tokeniseLine.IndexOf("#"));

                    if (s.Trim().EndsWith("="))
                        continue;
                    
                    var list = TokenizeLine(tokeniseLine);
                    tokens.AddRange(list);
                    tokeniseLine = "";

                }
            }
            else
            {
                current = script.Root;
            
                var list = TokenizeLine(line);
                tokens.AddRange(list);

            }
            DoLinesFromTokens(tokens, script);
            return script.Root;
        }

        public Script Load(string filename)
        {
            if (!File.Exists(filename))
            {

                Script ss = new Script();
                ss.Name = filename;
                ss.Root = new ScriptScope();
                return ss;
            }
            System.IO.StreamReader file =
               new System.IO.StreamReader(filename, Encoding.GetEncoding(1252));
            string line = "";
            Script script = new Script();
            script.Name = filename;
            script.Root = new ScriptScope();
            current = script.Root;
            List<String> tokens = new List<string>();
            String tokeniseLine = "";
            while ((line = file.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("log ="))
                {
                    continue;
                }
                if (line.Trim().StartsWith("#"))
                {
                    continue;
                }
                tokeniseLine += line.Trim();

                if (line.Trim().EndsWith("="))
                    continue;

                var list = TokenizeLine(tokeniseLine);
                tokens.AddRange(list);

                tokeniseLine = "";
            }

            DoLinesFromTokens(tokens, script);
            file.Close();

            return script;
        }

        private void DoLinesFromTokens(List<string> tokens, Script script)
        {
            String lineForm = "";
            for (int index = 0; index < tokens.Count; index++)
            {

                var token = tokens[index];
                if (index == 40700)
                {
                    
                }
                if (token.Contains("e_wendish_empire"))
                {
                    
                }
                if (token == "c_merv")
                {
                    int gfd = 0;
                }
                var next = "";
                var last = "";

                if (index > 0)
                    last = tokens[index - 1];
                if (index < tokens.Count - 1)
                    next = tokens[index + 1];
                lineForm += token + " ";
                if (token == "color" || token == "color2")
                {
                    if (tokens[index + 2].Trim() == "0")
                    {
                        try
                        {
                            Convert.ToInt32(tokens[index + 3]);
                        }
                        catch (Exception e)
                        {
                            lineForm += tokens[index + 1] + " ";
                            lineForm += tokens[index + 2] + " ";
                            index += 2;
                            DoLine(script, lineForm);
                            lineForm = "";
                            continue;
                        }
                    }
                    lineForm += tokens[index + 1] + " ";
                    lineForm += tokens[index + 2] + " ";
                    lineForm += tokens[index + 3] + " ";
                    lineForm += tokens[index + 4] + " ";
                    lineForm += tokens[index + 5] + " ";
                    index += 5;
                    DoLine(script, lineForm);
                    lineForm = "";
                    continue;
                    
                }
                if (token == "= {")
                {
                    DoLine(script, lineForm);
                    lineForm = "";
                }
                if (token == "}")
                {
                    DoLine(script, token);
                    lineForm = "";
                }
                if (last == "=")
                {
                    DoLine(script, lineForm);
                    lineForm = "";
                }
                if (last == ">=" || last == "<=" || last == "==")
                {
                    DoLine(script, lineForm);
                    lineForm = "";
                }
                if (next == "}")
                {
                    DoLine(script, lineForm);
                    lineForm = "";
                }
            }
        }

        public Script LoadKey(string key)
        {
            String filename = ModManager.instance.FileMap[key];
            if (!File.Exists(filename))
            {
                //  var f = File.CreateText(filename);
                //  f.Close();
                Script ss = new Script();
                ss.filenameIsKey = true;
                ss.Name = key;
                ss.Root = new ScriptScope();
                return ss;
            }
            System.IO.StreamReader file =
               new System.IO.StreamReader(filename, Encoding.GetEncoding(1252));
            string line = "";
            Script script = new Script();
            script.filenameIsKey = true;
            script.Name = key;
            script.Root = new ScriptScope();
            current = script.Root;
            int l = 0;
            List<String> tokens = new List<string>();
            String tokeniseLine = "";
            while ((line = file.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("log = "))
                {
                    continue;
                }
                if (line.Trim().Contains("#"))
                {
                    var split = line.Split('#');
                    if (split.Length > 1)
                    {
                        line = split[0].Trim();

                    }
                }
  
                tokeniseLine += line.Trim();

                if (line.Trim().EndsWith("="))
                    continue;

                var list = TokenizeLine(tokeniseLine);
                tokens.AddRange(list);

                tokeniseLine = "";
            }

            DoLinesFromTokens(tokens, script);

            file.Close();

            return script;
        }

        private ScriptScope current = null;
        public String deferString = "";
        private void DoLine(Script script, string line)
        {
            if (line.Contains("b_vakash"))
            {
                
            }
            line = line.Trim();
 
            if (deferString.Length > 0)
            {
                line = deferString + line;
                deferString = "";
            }
            if (line.Contains("{") && line.Contains("}"))
            {
                int open = line.Count("{");
                int closed = line.Count("}");

                if (closed < open)
                {
                    deferString += line.Trim();
                    return;
                }
                String t = line.Trim();
                if (t.StartsWith("#"))
                    return;
                int st = t.IndexOf('{');
                int en = t.LastIndexOf('}');
                String stripbe = t.Substring(0, st + 1);
                String strip = t.Substring(st + 1, (en) - (st + 1));
                String stripend = t.Substring(en);
                if (!stripbe.StartsWith("color"))
                {
                    DoLine(script, stripbe);
                    DoLine(script, strip);
                    DoLine(script, stripend);
                    return;

                }

            }
            if (line.Contains("{"))
            {
                if (line.Split('{').Length > 2 && line.Trim().EndsWith("{"))
                {
                    String t = line.Trim();
                    String[] spl = t.Split('{');
                    foreach (var s in spl)
                    {
                        if (s.Trim().Length > 0)
                            DoLine(script, s + "{");


                    }

                    return;

                }
            }

            bool skipping = false;
       
            String origline = line;
            lineNum++;
            if (line.Trim().Length == 0)
                return;
            if (line.Trim().StartsWith("#"))
                return;
         
            if (line.Trim().StartsWith("{"))
                return;
            if (line.Contains("#"))
            {
                line = line.Split('#')[0].Trim();
            }
            if (line.Contains('='))
            {
                String orig = line;
                String op = "=";
                if (line.Contains("=="))
                {
                    line = line.Replace("==", "=");
                    op = "==";
                }
                if (line.Contains(">="))
                {
                    line = line.Replace(">=", "=");
                    op = ">=";
                }
                if (line.Contains("<="))
                {
                    line = line.Replace("<=", "=");
                    op = "<=";
                }

                var sp = line.Split('=');
                if (sp.Length > 2)
                {
                    String newStart = sp[0] + op;
                    String newStart2 = line.Substring(line.IndexOf(op) + op.Length);
                    String newStart3 = null;
                    if (!newStart2.Trim().StartsWith("{"))
                    {
                        newStart += newStart2.Trim().Substring(0, newStart2.Trim().IndexOf(' '));
                        newStart2 = newStart2.Trim().Substring(newStart2.Trim().IndexOf(' '));
                        DoLine(script, newStart);
                        DoLine(script, newStart2);
                     
                        return;
                    }
                }
                line = orig;
            }
            if (line.Contains("="))
            {
                String op = "=";
                if (line.Contains("=="))
                {
                    line = line.Replace("==", "=");
                    op = "==";
                }
                if (line.Contains(">="))
                {
                    line = line.Replace(">=", "=");
                    op = ">=";
                }
                if (line.Contains("<="))
                {
                    line = line.Replace("<=", "=");
                    op = "<=";
                }
                string[] sp =  line.Split('=');
                string name = sp[0].Trim();
 
                if (!(line.Contains("{") || sp[1].Trim().Length == 0))
                {
                    string value = sp[1].Trim();
                    while(value.EndsWith("}"))
                        value = value.Substring(0, value.Length-1).Trim();

                    if (line.EndsWith("}"))
                    {
                        DoLine(script, line.Substring(0, line.Length - 1));
                        DoLine(script, "}");
                        return;
                    }
                    if (sp.Length > 2)
                    {
                        
                    }
                    if (value.Contains("["))
                    {
                     
                        if (sp.Count() > 2)
                        {

                        } 
                        value = sp[1].Replace("[", "");
                        object val = GetValueFromString(value);
                        current.Add(new ScriptCommand() { Name = name, Value = val, AlwaysQuote=true, Op=op });
        
                    }
                    else
                    {
                        object val = GetValueFromString(value);
                        current.Add(new ScriptCommand() { Name = name, Value = val, Op = op });
     
                    }

                }
            
                else if (!line.StartsWith("}"))
                {
                  
                    if (line.Contains("{") && line.Contains("}"))
                    {
                        string sname = line.Split('=')[0].Trim();
                        if (sname == "color" || sname == "color2")
                        {
                            object val = GetValueFromString(line.Split('{', '}')[1].Trim());
                           
                            current.Add(new ScriptCommand() { Name = name, Value = val });
                            return;
                        }
                        
                        var s = new ScriptScope() { Name = name };
                        s.Parent = current;
                        scopeStack.Push(current);
                        current.Add(s);
                        current = s;

                        int st = line.IndexOf('{');
                        int en = line.LastIndexOf('}');
                        String strip = line.Substring(st + 1, en - (st + 1));
                        bool bDone = false;
                   
                        if (strip.Contains("{") && strip.Contains("="))
                        {
                            int brace = strip.IndexOf("{");
                            int eq = strip.IndexOf("=");
                            String between = strip.Substring(eq+1, brace - eq - 1);
                            if (between.Trim().Length > 0)
                            {
                                String[] sp2 = strip.Split(new[] { ' ', '\t' });
                                List<String> lines = new List<string>();

                                foreach (var s1 in sp2)
                                {
                                    if (s1.Trim().Length == 0)
                                        continue;
                                    
                                    bool hasSomethingElse = false;
                                    if(s1.Contains("}"))
                                    {
                                        String s2 = s1.Trim();
                                        String str = "";
                                        for (int index = 0; index < s2.Length; index++)
                                        {
                                            
                                            var c = s2[index];
                                            if (c == '}')
                                            {
                                                if (str.Trim().Length > 0)
                                                {
                                                    lines.Add(str.Trim());
                                                }
                                                lines.Add("}");
                                                
                                                str = "";
                                            }
                                            else
                                            {
                                                hasSomethingElse = true;
                                                str += c.ToString();
                                            }
                                        }
                                    }
                                }

                                List<String> comp = new List<string>();
                                for (int index = 0; index < lines.Count-1; index++)
                                {
                                    var line1 = lines[index];
                                    if (lines[index + 1] == "{")
                                    {
                                        String tot = "";
                                        for (int ii = index + 2; ii < lines.Count; ii++)
                                        {
                                            tot += lines[ii];
                                        }
                                        if (tot.Contains("6032"))
                                        {

                                        }
                                        DoLine(script, tot.Substring(0, tot.Length));
                                    }
                                    if (line1 == "=")
                                    {
                                        comp.Add(lines[index - 1] + " " + lines[index] + " " + lines[index + 1]);
                                    }
                                }

                                foreach (var l in comp)
                                {
                                
                                    DoLine(script, l.Trim());
                                }

                            }
                            else
                            {
                                DoLine(script, strip.Trim());
                            }
                        }
                        else
                        {
                            {
                                String[] sp2 = strip.Split(new[] { ' ', '\t' });
                                List<string> lines = new List<string>();
                                
                                foreach (var s1 in sp2)
                                {
                                    if (s1.Trim().Length == 0)
                                        continue;

                                    lines.Add(s1.Trim());
                                }

                                List<String> comp = new List<string>();

                                for (int index = 0; index < lines.Count - 1; index++)
                                {
                                    var line1 = lines[index];
                                    if (line1 == "=")
                                    {
                                        comp.Add(lines[index - 1] + " " + lines[index] + " " + lines[index + 1]);
                                    }
                                
                                }
                                if (comp.Count == 0 && strip.Trim().Length > 0)
                                {
                                    DoLine(script, strip);


                                }
                                foreach (var l in comp)
                                {
                                    DoLine(script, l.Trim());
                                }
                            }

                           // DoLine(script, strip.Trim());
        
                        }

                  
                        current = scopeStack.Pop();

                        return;

                    }
                    string sname2 = line.Split('=')[0].Trim();
                    if (sname2 == "allow")
                    {
                        
                    }
                    ScriptScope scope = new ScriptScope();
                    scope.Parent = current;
                    scope.Name = sname2;

                    scopeStack.Push(current);
                    current.Add(scope);

              
                    current = scope;
                    current.Name = name;

                }
            }
            else if (line.Trim().StartsWith("}"))
            {
                if (scopeStack.Count == 0)
                {
                    line = line.Trim().Substring(1);
                    DoLine(script, line);
                    return;
                }
                current = scopeStack.Pop();
                line = line.Trim().Substring(1);
                DoLine(script, line);
            }
            else
            {
                current.Data += origline + "\n";
            }
           
        }

        public static object GetValueFromString(string value)
        {
            if(value.ToString().Trim() == "\"\"")
                return "\"\"";
            if (value.ToString().Trim().Length == 0)
                return "\"\"";
            
            value = value.Trim();
            if (value == "yes" || value == "no")
                return value == "yes";
            if (value.Contains("{"))
            {
                value = value.Replace("{", "");
                value = value.Replace("}", "");
                value = value.Trim();
            }
            if (value.Split(' ').Count() >= 3 && !value.StartsWith("\""))
            {

                bool isFloat = value.Contains(".");
                var sp = value.Split(' ');
                try
                {
                    if(isFloat)
                        return Color.FromArgb(255, (int) (255.0f * Convert.ToSingle(sp[0])), (int) (255.0f * Convert.ToSingle(sp[1])), (int) (255.0f * Convert.ToSingle(sp[2])));
                    else
                        return Color.FromArgb(255, Convert.ToInt32(sp[0]), Convert.ToInt32(sp[1]), Convert.ToInt32(sp[2]));
                }
                catch (Exception ex)
                {
                    try
                    {
                        int[] rgb = new int[3];
                        int c = 0;

                        foreach (var s in sp)
                        {
                            if (s.Trim().Length > 0)
                            {
                                rgb[c] = Convert.ToInt32(s.Trim());
                                c++;
                                if (c >= 3)
                                    break;
                            }
                        }

                        return Color.FromArgb(255, rgb[0], rgb[1], rgb[2]);
                    }
                    catch (Exception)
                    {
                        
                        
                    }

                }
            }
            if(value.Contains("\""))
                return value.Replace("\"", "");
            return value;
        }

    }
}
