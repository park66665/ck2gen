using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class ScopeProxy
    {
        ScriptScope scope;
        public virtual void Init(ScriptScope scope)
        {
            this.scope = scope;
        }

        public ScopeProxy()
        {
            
        }

        public string GetString(String name)
        {
            return (scope.ChildrenMap[name] as ScriptCommand).Value.ToString();
        }
        public bool GetBool(String name)
        {
            return ((bool)(scope.ChildrenMap[name] as ScriptCommand).Value);
        }
        public int GetInt(string name)
        {
            return ((int)(scope.ChildrenMap[name] as ScriptCommand).Value);

        }

        public void SetValue(String name, object value)
        {
            if (!scope.ChildrenMap.ContainsKey(name))
            {
                var c = new ScriptCommand(name, value, scope);
                scope.Add(c);
                
            }
            else
            {
                (scope.ChildrenMap[name] as ScriptCommand).Value = value;
            }
            
        }
   
    }
}
