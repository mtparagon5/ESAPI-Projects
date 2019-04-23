﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMS.TPS
{
  public class CleanString
  {
    public static string clean(string stringToClean){
      var sb = new StringBuilder(stringToClean.Length);

      foreach (char i in stringToClean)
          if (i != '\n' && i != '\r' && i != '\t')
              sb.Append(i);

      var cleanedString = sb.ToString();

      return cleanedString;
    }
  }
}
