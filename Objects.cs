using System;
using System.Linq;
using System.Collections.Generic;

using DDW; //csparser

namespace aaa
{
	public partial class MainClass
	{

		//constants are handled nicely by csparser
		public static dictionary<string, Class> classes;
		public static ObjectInstance executingClass;
		public static Function executingFunction;
		public static string localVarDeclarationName;



		static void setup(CompilationUnitNode cu)
		{
			foreach(var ns in cu.Namespaces)
			{
				foreach(var cs in ns.Classes)
				{
					classes[cs.Name.Identifier] = new Class(){ classnode = cs };
				}

				foreach(var cs in ns.Classes)
				{
					var cls = classes[cs.Name.Identifier];

					//"register" the class function symbols
					foreach(var method in cs.Methods)
					{
						var f = new Function(){method = method, ownerClass=cls};
						f.Params = new Parameters();

						//!!! we assume csparser has given us the args in the right order (we establish both the string name key AND integer index access methods)
						foreach(var param in method.Params)
						{
							string typename = ((dynamic)param).Type.Identifier.GenericIdentifier;
							f.Params[param.Name] = new ObjectInstance(typename, param.Name);
						}

						f.Params.isInit=false;
						cls.functions[f.name] = f;
						cls._functionOverloads.Add(f);
					}
				}

				foreach(var cs in ns.Classes)
				{
					var cls = classes[cs.Name.Identifier];

					cls.initFields(staticInit: true);
				}


			}

		}



		//no structs
		static List<string> nonclass_types = new string[]{ "int","bool","float","double","string","string[]","long","char", }.ToList();



		public class Function
		{
			public string name { get { return method.GenericIdentifier.Split(' ')[1]; } }
			public Class ownerClass;
			public MethodNode method;
			public Parameters Params;

			public dictionary<string, ObjectInstance> Localvars;
			public dynamic returnValue;


			//This is all very delicate form.
			public dynamic _call(List<dynamic> args, ObjectInstance fnScopeExecutingClassInstance = null)
			{
				var callerFn = executingFunction;
				var callerInst = executingClass;
				
				executingFunction = this; //push to the function stack
				executingClass = fnScopeExecutingClassInstance ?? executingClass;
				print("entering function: " + name);


				if(method.Params.Count != args.Count) throw new Exception("Wrong number of arguments");
				
				//linkup the arguments
				for(int i=0; i<args.Count; i++) 
					Params[i].value = args[i]; 


				//collapse a series of statements down as trees of evaluation
				foreach(var sta in method.StatementBlock.Statements)
				{
					if(returnValue != null) break;

					handle_dyn(sta); //do statement. Early return might be done here
				}

				executingFunction = callerFn; //pop the function stack
				executingClass = callerInst; //pop class member entry stack


				return returnValue;
			}





			//wrapper function to support function overloading.
			public dynamic call(List<dynamic> args, ObjectInstance fnScopeExecutingClassInstance = null)
			{
				
				//find and call the right overload
				var overloads = ownerClass._functionOverloads.Where(f=>f.name == name);
				if(overloads.Count() == 1){ return clone()._call(args, fnScopeExecutingClassInstance); }
					
				foreach(var ovl in overloads)
				{
					if(args.Count != ovl.Params.items.Count) continue;

					bool parametersFit=true;
					//for(int i=0; i<ovl.Params.items.Count; i++)
					//	if(!canDynamicCast(args[i], ovl.Params[i].GetType())) parametersFit=false;

					if(parametersFit) return ovl.clone()._call(args, fnScopeExecutingClassInstance);
				}

				throw new Exception("couldn't find the function overload");
			}




			//The function when called (again further up the stack!) shouldn't be messing with the same Localvars or Params pointers/memory.
			//We linkup the essential opaque data to the new instance. The rest is dynamically added.
			public Function clone(){ return new Function(){ method=method, ownerClass=ownerClass, Params=Params.clone() }; }


		}




		public class Class
		{
			public dictionary<string, Function> functions;
			public dictionary<string, ObjectInstance> staticfields;
			public dictionary<string, string> fieldNames;
			public List<Function> _functionOverloads = new List<Function>();


			public void initFields(ObjectInstance obj=null, bool staticInit=false)
			{

				//initialize class fields
				foreach(var field in classnode.Fields)
				{
					string typename = ((dynamic)field.Type).GenericIdentifier;


					var fieldname = field.Names[0].GenericIdentifier;

					if(staticInit && field.isStatic())
						this.staticfields[fieldname] = new ObjectInstance(typename, fieldname);
					if(!staticInit && typename != name)
						obj._fields[fieldname] = new ObjectInstance(typename, fieldname);
					if(!staticInit && typename == name)
						obj._fields[fieldname] = new ObjectInstance(typename, fieldname, allocate: false); //self-referential fields are allocated on demand

					fieldNames[fieldname] = fieldname;
				}

			}
			
			public DDW.ClassNode classnode;
			public string name{get{return classnode.Name.Identifier;}}

			//public Func<dynamic> constructor; //TODO: we need to dynamically create a new one
		}


		//class field, local variable etc.
		public class ObjectInstance
		{
			//If you don't want the self-referential class fields feature, you can trash these 3 lines and just have "public dynamic value;" (Same for "fields" field).
			public dynamic _value;
			public dynamic value { get { if(!init && Class!=null) { Class.initFields(this); init=true; } return _value; } set{ _value = value; } }
			public bool init;

			public ObjectInstance(string typename, string name, bool allocate=true)
			{
				this.typename=typename;
				this.name=name;
				
				if(typename.in_(nonclass_types))
				{
					//indicate type
					switch(typename)
					{
					case "int": { value=(int)0; break; }
					case "bool": { value=false; break; }
					case "float": { value=(float)0; break; }
					case "double": { value=(double)0; break; }
					case "string": { value=""; break; }
					case "string[]": { value=new string[]{""}; break; }
					case "long": { value=(long)0; break; }
					case "char": { value=(char)0; break; }
					default: throw new NotImplementedException();
					}
				}
				else //class or struct. Structs would have to be deep-copied on assignment
				{
					Class = classes[typename];

					//foreach nonstatic field in the class, we attach an object instance field by that name. (class fields' initializer syntax is not supported)
					if(allocate) Class.initFields(this);

				}
			}

			public static implicit operator Class(ObjectInstance o){ return o.Class; }


			public string typename;
			public string name;
			public bool isArray{ get{ return typename.Contains("[]"); } }

			public dictionary<string, ObjectInstance> _fields;
			public dictionary<string, ObjectInstance> fields { get { if(!init && Class!=null) { Class.initFields(this); init=true; } return _fields; } set{ _fields = value; } }

			public Class Class;
			public bool isPrimitiveInbuiltType { get{ return Class == null && typename != "dynamic"; } } //whatever


			//public Func<dynamic> constructor; //TODO: we need to dynamically create a new one, through some structure..
		}





		public class Parameters
		{
			public List<ValuePair<string, ObjectInstance>> items = new List<ValuePair<string, ObjectInstance>>();

			public bool isInit=true;

			public ObjectInstance this[string byName]  {  get  {  return items.Where(x => x.k == byName).First().value;  }  set  {  if(isInit)  items.Add(new ValuePair<string, ObjectInstance>(){k=byName, value=value});  else { var x = items.Where(i=>i.k==byName).First(); x.value=value; } }  }
			public ObjectInstance this[int byIndex] { get { return items[byIndex].value; } }

			//function params list constitutes a unique name, to differentiate function overloads
			public override string ToString () { string label="("; foreach(var p in items) {label += p.k + ", ";} label=label.Substring(0, label.Length-2); label+=")"; return label; }

			public bool has(string key){ return items.Where(i=>i.k==key).Count()>0; }


			public Parameters clone() { var Params = new Parameters(); foreach(var param in items) { var v = param.value; Params[v.name] = new ObjectInstance(v.typename, v.name); } return Params; }
		}


		public struct ValuePair<K,V> { public K k; public V value; }


		//all this does is avoids repeating the syntax, "dictionary<K,V> dict = new dictionary<K,V>()", this way it's clearer and easier to read.
		public struct dictionary<K,V> { public bool has(K k){ if(dict==null) return false; return dict.ContainsKey(k);} public Dictionary<K,V> dict; public V this[K key] { get { if(dict==null) dict = new Dictionary<K,V>(); return dict[key]; } set { if(dict==null) dict = new Dictionary<K,V>(); dict[key] = value; } } }






	}
}

