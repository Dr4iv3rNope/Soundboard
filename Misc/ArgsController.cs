using System.Collections.Generic;

namespace soundboard.Misc
{
	public struct ArgStruct
	{
		public string value;
		public string name;
	}

	public class ArgsController
	{
		//
		// arg classs
		//
		public class Arg
		{
			//
			// event
			//
			public delegate void IsArgPutFn(ArgStruct arg);
			public IsArgPutFn OnArgPut;

			public delegate void IsArgNoneFn();
			public IsArgNoneFn OnNoArg;

			// vars
			public string name { get; }
			public bool founded { get; set; } = false;

			// constructor
			public Arg(string name) => this.name = name;

			public Arg(string name, ref ArgsController controller, IsArgPutFn argPut = null, IsArgNoneFn argNone = null)
			{
				this.name = name;
				OnArgPut = argPut;
				OnNoArg = argNone;

				controller.AddArg(this);
			}
		}

		// values
		public char argStart { get; }

		public List<ArgStruct> argsList { get; }
		public List<Arg> argListen { get; }

		// constructor
		public ArgsController(char argStart, string[] args)
		{
			var argString = string.Join(" ", args).Split(argStart); 

			argsList = new List<ArgStruct>();

			ArgStruct sArg;
			sArg.name = null;
			sArg.value = null;

			foreach(string arg in argString)
			{
				string[] split = arg.Split(' ');

				sArg.name = split[0];
				sArg.value = split.Length > 1 ? split[1] : null;

				argsList.Add(sArg);
			}

			argListen = new List<Arg>();

			this.argStart = argStart;
		}

		// add arg
		public void AddArg(Arg arg)
		{
			argListen.Add(arg);
		}

		// check args
		public void CheckArgs()
		{
			foreach(var arg in argListen)
			{
				foreach (var _arg in argsList)
				{
					if(arg.name == _arg.name)
					{
						arg.OnArgPut?.Invoke(_arg);
						arg.founded = true;

						break;
					}
				}

				if (!arg.founded)
					arg.OnNoArg?.Invoke();
			}
		}
	}
}
