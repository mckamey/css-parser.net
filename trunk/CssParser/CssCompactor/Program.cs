#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion BuildTools License

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using BuildTools.IO;

namespace BuildTools.CssCompactor
{
	class Program
	{
		#region Constants

		private const string Help =
			"Cascading StyleSheet compactor and syntax validator (version {0})\r\n\r\n"+
			"CssCompactor /IN:file [ /OUT:file ] [ /INFO:copyright ] [ /TIME:timeFormat ] [ /P ]\r\n\r\n"+
			"\t/IN:\tInput File Path\r\n"+
			"\t/OUT:\tOutput File Path\r\n"+
			"\t/INFO:\tCopyright label\r\n"+
			"\t/TIME:\tTimeStamp Format\r\n"+
			"\t/P\tPretty-Print the output (default is compact)\r\n\r\n"+
			"e.g. CssCompactor /IN:myFile.css /OUT:compacted/myFile.css /INFO:\"(c)2007 My CSS\" /TIME:\"'Compacted 'yyyy-MM-dd @ HH:mm\"";

		private enum ArgType
		{
			Empty,// need a default value
			InputFile,
			OutputFile,
			Copyright,
			TimeStamp,
			PrettyPrint
		}

		private static readonly ArgsTrie<ArgType> Mapping = new ArgsTrie<ArgType>(
			new ArgsMap<ArgType>[] {
				new ArgsMap<ArgType>("/IN:", ArgType.InputFile),
				new ArgsMap<ArgType>("/OUT:", ArgType.OutputFile),
				new ArgsMap<ArgType>("/INFO:", ArgType.Copyright),
				new ArgsMap<ArgType>("/TIME:", ArgType.TimeStamp),
				new ArgsMap<ArgType>("/P", ArgType.PrettyPrint)
			});

		#endregion Constants

		#region Program Entry

		static void Main(string[] args)
		{
			try
			{
				Dictionary<ArgType, string> argList = Mapping.MapAndTrimPrefixes(args);

				string inputFile = argList.ContainsKey(ArgType.InputFile) ? argList[ArgType.InputFile] : null;
				string outputFile = argList.ContainsKey(ArgType.OutputFile) ? argList[ArgType.OutputFile] : null;
				string copyright = argList.ContainsKey(ArgType.Copyright) ? argList[ArgType.Copyright] : null;
				string timeStamp = argList.ContainsKey(ArgType.TimeStamp) ? argList[ArgType.TimeStamp] : null;
				CssCompactor.Options options = argList.ContainsKey(ArgType.PrettyPrint) ?
					CssCompactor.Options.PrettyPrint|CssCompactor.Options.Overwrite : CssCompactor.Options.Overwrite;

				if (String.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
				{
					Console.Error.WriteLine(Program.Help, Assembly.GetExecutingAssembly().GetName().Version);
					return;
				}

				List<ParseException> errors;
				if (String.IsNullOrEmpty(outputFile))
				{
					errors = CssCompactor.Compact(inputFile, null, Console.Out, copyright, timeStamp, options);
				}
				else
				{
					errors = CssCompactor.Compact(inputFile, outputFile, copyright, timeStamp, options);
				}

				if (errors.Count > 0)
				{
					foreach (ParseException ex in errors)
					{
						Console.Error.WriteLine(ex.GetCompilerMessage());
					}
				}
				else
				{
					FileInfo inputInfo = new FileInfo(inputFile);
					FileInfo outputInfo = new FileInfo(outputFile);

					Console.WriteLine("Compacted \"{0}\" by {1:P1}", inputInfo.Name, (Decimal.One - (((decimal)outputInfo.Length)/((decimal)inputInfo.Length))));
				}
			}
			catch (ParseException ex)
			{
				Console.Error.WriteLine(ex.GetCompilerMessage());
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
			}
		}

		#endregion Program Entry
	}
}
