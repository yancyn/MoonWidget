using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MyClass
{
	public static void Main()
	{
		RL();
	}
	
	private void RenameToSmallLetter(string sourcePath)
		{
			try
			{
				System.IO.DirectoryInfo dinfo = new System.IO.DirectoryInfo(sourcePath);
				System.IO.FileInfo[] finfos = dinfo.GetFiles();
				
				foreach(System.IO.FileInfo f in finfos)
				{
					string fileNameOnly = GetFileNameOnly(f.Name);
					string newFileName = CastTo(fileNameOnly);
					
					string newLocation = sourcePath+"2"+"\\"+newFileName+f.Extension;
					if(!System.IO.File.Exists(newLocation))
					{
						f.CopyTo(newLocation);
						//f.Delete();
					}
				}//end loops
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
				return;
			}
		}
		private string GetFileNameOnly(string sender)
		{
			string output = sender;
			int i = sender.LastIndexOf(".");
			
			output = sender.Substring(0,i);
			return output;
		}
		private string CastTo(string sender)
		{
			string output = sender;
			
			try
			{
				
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
				return output;
			}
			
			return output;
		}
		 public bool IsInteger(string val)
        {
            if (val.Trim().Length == 0) return false;

            /* foreach(char c in val)
            {
                bool lb_Valid = false;
                if(c.ToString() == "0" || c.ToString() == "1" || c.ToString() == "2"
                    || c.ToString() == "3" || c.ToString() == "4" || c.ToString() == "5"
                    || c.ToString() == "6" || c.ToString() == "7" || c.ToString() == "8"
                    || c.ToString() == "9" )
                {
                    lb_Valid = true;
                    continue;
                }

                if(!lb_Valid)
                    return false;//not a valid integer found
            }//end loops string

            return true; */

            Regex objNotIntPattern = new Regex("[^0-9-]");
            Regex objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
            return !objNotIntPattern.IsMatch(val) && objIntPattern.IsMatch(val);
        }
	
	#region Helper methods

	private static void WL(object text, params object[] args)
	{
		Console.WriteLine(text.ToString(), args);	
	}
	
	private static void RL()
	{
		Console.ReadLine();	
	}
	
	private static void Break() 
	{
		System.Diagnostics.Debugger.Break();
	}

	#endregion
}