using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Plexus.Utils
{
    public class Validator
    {
        /// <summary>
        /// Validation class.
        /// </summary>
        public Validator()
        {
        }

        /// <summary>
        /// Check whether is a valid integer or number.
        /// <see href="http://www.c-sharpcorner.com/UploadFile/prasad_1/RegExpPSD12062005021717AM/RegExpPSD.aspx">
        /// Regular Expressions Usage in C#</see>
        /// <seealso cref="IsASCII" />
        /// </summary>
        /// <param name="sender">Pass in value to check.</param>
        /// <returns>True is a valid integer otherwise false.</returns>
        /// <remarks>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2008-07-18<br/>
        /// <b>Changes</b>  Use regex method [2008-07-22: yeang-shing.then]
        /// </remarks>
        /// <example>
        /// <code>
        /// if(IsInteger("4a")) {
        /// }
        /// </code>
        /// </example>
        public static bool IsInteger(string sender)
        {
            if (sender == null) return false;
            if (sender.Trim().Length == 0) return false;
            Regex objNotIntPattern = new Regex("[^0-9-]");
            Regex objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
            return !objNotIntPattern.IsMatch(sender) && objIntPattern.IsMatch(sender);
        }
        /// <summary>
        /// Check whether is a valid money format.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <see>http://regexlib.com/DisplayPatterns.aspx?categoryId=3&cattabindex=3</see>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-04-29<br/>
        /// </remarks>
        public static bool IsMoney(string sender)
        {
            if (sender == null) return false;
            if (sender.Trim().Length == 0) return false;
            Regex regex = new Regex("^(\\d|-)?(\\d|,)*\\.?\\d*$");//"^[-+]?\\d*\\.?\\d*$");
            return regex.IsMatch(sender);
        }
        /// <summary>
        /// Check whether is a valid date time value.
        /// </summary>
        /// <param name="val">String value to validate</param>
        /// <returns>True is a valid date time otherwise false.</returns>
        public static bool IsDateTime(string val)
        {
            try
            {
                Convert.ToDateTime(val);
                return true;

                //Regex r = new Regex("\\d{1,2}\\/\\d{1,2}/\\d{4}");
                //return r.IsMatch(val);
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                return false;
            }
        }
        /// <summary>
        /// Return true if it is a valid US short date format(MM/dd/yyyy).
        /// This regex will match SQL Server datetime values, allowing date only,
        /// allowing zero padded digits in month, day and hour,
        /// and will match leap years from 1901 up until 2099.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>True if valid otherwise false</returns>
        /// <seealso>http://regexlib.com/DisplayPatterns.aspx?cattabindex=4&categoryId=5</seealso>
        /// <remarks>
        /// <b>Since</b>	2009-05-27<br/>
        /// </remarks>
        public static bool IsDateTimeWithMonthDayYear(string sender)
        {
            if (sender == null) return false;
            if (sender.Trim().Length == 0) return false;
            //Regex regex = new Regex("^([1-9]|0[1-9]|1[012])[- /.]([1-9]|0[1-9]|[12][0-9]|3[01])[- /.][0-9]{4}$");
            Regex regex = new Regex("^((((((0?[13578])|(1[02]))[\\-\\/\\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\\-\\/\\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\\-\\/\\s]?((0?[1-9])|([1-2][0-9]))))[\\-\\/\\s]?\\d{2}(([02468][048])|([13579][26])))|(((((0?[13578])|(1[02]))[\\-\\/\\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\\-\\/\\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\\-\\/\\s]?((0?[1-9])|(1[0-9])|(2[0-8]))))[\\-\\/\\s]?\\d{2}(([02468][1235679])|([13579][01345789]))))(\\s(((0?[1-9])|(1[0-2]))\\:([0-5][0-9])((\\s)|(\\:([0-5][0-9])\\s))([AM|PM|am|pm]{2,2})))?$");
            return regex.IsMatch(sender);
        }

        /// <summary>
        /// Determine whether is absolute contain ASCII only.
        /// </summary>
        /// <param name="as_body"></param>
        /// <returns></returns>
        public static bool IsASCII(string as_body)
        {
            Encoding encoding1 = Encoding.Unicode;
            Encoding encoding2 = Encoding.ASCII;
            byte[] buffer1 = encoding1.GetBytes(as_body);
            byte[] buffer2 = Encoding.Convert(encoding1, encoding2, buffer1);
            char[] chArray1 = new char[encoding2.GetCharCount(buffer2, 0, buffer2.Length)];
            encoding2.GetChars(buffer2, 0, buffer2.Length, chArray1, 0);
            string text1 = new string(chArray1);
            if (as_body.Equals(text1))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Compare two string value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsEqual(string source, string target)
        {
            if (source == target)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Compare two value.
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="col">Column Name.</param>
        /// <param name="val">Value to compare.</param>
        /// <returns>True if equal otherwise false.</returns>
        public static bool IsEqual(DataRow row, string col, object val)
        {
            if (row[col].GetType() == typeof(bool))
            {
                bool sender = false;
                if (val.GetType() == typeof(int))
                    sender = Convert.ToInt32(val) == 1 ? true : false;
                else if (val.GetType() == typeof(bool))
                    sender = (bool)val;

                if ((bool)row[col] == sender)
                    return true;
                else
                    return false;
            }
            else if (row[col].GetType() == typeof(string))
            {
                if (row[col].ToString().Trim() == val.ToString().Trim())
                    return true;
                else
                    return false;
            }
            else if (row[col].GetType() == typeof(int))
            {
                if (!IsInteger(val.ToString())) return false;
                if (Convert.ToInt32(row[col]) == Convert.ToInt32(val))
                    return true;
                else
                    return false;
            }
            else if (row[col].GetType() == typeof(decimal))
            {
                if (Convert.ToDecimal(row[col]) == Convert.ToDecimal(val))
                    return true;
                else
                    return false;
            }
            else if (row[col].GetType() == typeof(DateTime))
            {
                if (!IsDateTime(val.ToString())) return false;
                if (Convert.ToDateTime(row[col]) == Convert.ToDateTime(val))
                    return true;
                else
                    return false;
            }


            return false;
        }
        /// <summary>
        /// Compare joined/mixed string with separator specified.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static bool IsEqual(string source, string target, char seperator)
        {
            if (source == null || target == null) return true;
            if (source.Trim() == string.Empty || target.Trim() == string.Empty) return true;

            source = source.Trim().TrimStart(seperator).TrimEnd(seperator);
            string[] sources = source.Split(seperator);
            DataManipulation.BubbleSort(sources);
            source = "";
            foreach (string s in sources)
                source += s.Trim() + seperator.ToString();

            target = target.Trim().TrimStart(seperator).TrimEnd(seperator);
            string[] targets = target.Split(seperator);
            DataManipulation.BubbleSort(targets);
            target = "";
            foreach (string s in targets)
                target += s.Trim() + seperator.ToString();

            if (source == target)
                return true;
            else
                return false;
        }

        /// <summary>
        /// True if the object is empty otherwise false.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static bool IsEmpty(object sender)
        {
            bool empty = false;
            if (sender == null)
                empty = true;
            else
            {
                if (sender.GetType() == typeof(string))
                {
                    if (sender.ToString().Trim() == string.Empty)
                        empty = true;
                }
                else if (sender.GetType() == typeof(bool))
                {
                    if ((bool)sender == false)
                        empty = true;
                }
                else if (sender.GetType() == typeof(System.Int16))
                {
                    if ((System.Int16)sender == 0) empty = true;
                }
                else if (sender.GetType() == typeof(System.Int32))
                {
                    if ((System.Int32)sender == 0) empty = true;
                }
                else if (sender.GetType() == typeof(System.Int64))
                {
                    if ((System.Int64)sender == 0) empty = true;
                }
            }

            //logger.Info("IsEmpty(" + sender + ")=" + empty);
            return empty;
        }
        /// <summary>
        /// Validate is a valid email address or not.
        /// </summary>
        /// <param name="inputEmail">Email address to check.</param>
        /// <returns>True if valid otherwise false.</returns>
        public static bool IsEmail(string inputEmail)
        {
            //Regex emailregex = new Regex("(?<user>[^@]+)@(?<host>.+)");
            //return emailregex.IsMatch(val);

            string text1 = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex regex1 = new Regex(text1);
            if (regex1.IsMatch(inputEmail))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check whether is a valid password format (must alpha numeric, and consist funny character eg. #,$).
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1. At least 8 characters long.
        /// 2. Contain Alpha character A-Z or a-z.
        /// 3. Contain numeric character 0-9.
        /// 4. Contain special character eg. *,$,), etc.
        ///		- UNIX not support: @,#
        ///		- Win2000 only support: ! @ #  $ % ^ & * 
        ///	5. Must not contain space.
        ///	6. Must not same as login ID.
        ///	
        ///	
        ///	- Reset password after 90 days.
        ///	- Can not be same as previous password.
        /// </remarks>
        public static bool IsPassword(string val)
        {
            //string space = " ";
            bool hasInteger = false;
            bool hasRoman = false;
            bool hasFunny = false;
            bool hasAscii = false;

            #region remark old method
            /* string[] funny = new string[]{
											 "~","!","@","#","$","%","^","&","*","(",")","_","+",
											 "`","-","=",
											 "{","}","|",
											 "[","]","\\",
											 ":","\"",
											 ";","'",
											 "<",">","?",
											 ",",".","/"											 
										 };
			

				foreach(char c in val)
				{
					if(IsInteger(c.ToString()))
						hasInteger = true;
					else
						hasRoman = true;
				}

				foreach(char c in val)
				{
					foreach(string f in funny)
					{
						if(c.ToString() == space) return false;
						if(c.ToString()==f)
						{
							hasFunny = true;
							break;
						}
					}
				}

				if(!hasFunny)
				{
					
					string[] ascii = new string[]{
													 "\u00A1","\u00A2","\u00A3","\u00A4","\u00A5","\u00A6","\u00A7","\u00A8","\u00A9","\u00AA","\u00AB","\u00AC","\u00AD","\u00AE","\u00AF",
													 "\u00B0","\u00B1","\u00B2","\u00B3","\u00B4","\u00B5","\u00B6","\u00B7","\u00B8","\u00B9","\u00BA","\u00BB","\u00BC","\u00BD","\u00BE","\u00BF",
													 "\u00C0","\u00C1","\u00C2","\u00C3","\u00C4","\u00C5","\u00C6","\u00C7","\u00C8","\u00C9","\u00CA","\u00CB","\u00CC","\u00CD","\u00CE","\u00CF",
													 "\u00D0","\u00D1","\u00D2","\u00D3","\u00D4","\u00D5","\u00D6","\u00D7","\u00D8","\u00D9","\u00DA","\u00DB","\u00DC","\u00DD","\u00DE","\u00DF",
													 "\u00E0","\u00E1","\u00E2","\u00E3","\u00E4","\u00E5","\u00E6","\u00E7","\u00E8","\u00E9","\u00EA","\u00EB","\u00EC","\u00ED","\u00EE","\u00EF",
													 "\u00F0","\u00F1","\u00F2","\u00F3","\u00F4","\u00F5","\u00F6","\u00F7","\u00F8","\u00F9","\u00FA","\u00FB","\u00FC","\u00FD","\u00FE","\u00FF"
												 };

					foreach(char c in val)
					{
						foreach(string a in ascii)
						{
							if(c.ToString()==a)
							{
								hasAscii = true;
								break;
							}
						}
					}
				} */
            #endregion

            try
            {
                System.Text.Encoding ascii = System.Text.Encoding.ASCII;
                Byte[] encodeBytes = ascii.GetBytes(val);

                /* !"#$%&`()*+,-./
                 * :;<=>?@
                 * [\]^_'
                 * {|}~
                 *
                 * ascii 32 = space
                 * ascii 33~47, 58~64, 91~96, 123~126
                 * 
                 * 0~9
                 * ascii 48~57
                 * 
                 * A~Z, a~z
                 * ascii 65~90, 97~122
                 * 
                 * */



                foreach (byte b in encodeBytes)
                {
                    if (b == 32) return false;

                    if (b >= 48 && b <= 57)
                        hasInteger = true;

                    if ((b >= 65 && b <= 90) || (b >= 97 && b <= 122))
                        hasRoman = true;

                    if ((b >= 33 && b <= 47)
                        || (b >= 58 && b <= 64)
                        || (b >= 91 && b <= 96)
                        || (b >= 123 && b <= 126))
                        hasFunny = true;
                }//end loops



                if (hasInteger && hasRoman && (hasFunny | hasAscii))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                //logger.Error(ex);
                return false;
            }
        }
        /// <summary>
        /// Check whether is a valid password format (must alpha numeric, and consist funny character eg. #,$).
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min">Minimum length.</param>
        /// <returns></returns>
        public static bool IsPassword(string val, int min)
        {
            if (val == String.Empty)
                return false;

            if (val.Length < min)
                return false;

            return IsPassword(val);
        }
        /// <summary>
        /// Check whether is a valid password format (must alpha numeric, and consist funny character eg. #,$).
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min">Minimum length.</param>
        /// <param name="loginID">Login ID (can not same with password).</param>
        /// <returns></returns>
        public static bool IsPassword(string val, int min, string loginID)
        {
            if (val == String.Empty)
                return false;

            if (val.Length < min)
                return false;

            if (val == loginID)
                return false;

            return IsPassword(val);
        }

    }
}