namespace VMS.TPS
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// ESJO is short for Eclipse Scripting JSON Object. The ESJO Class can be used to create nested JSON objects from plan data.
	/// These JSON objects can then be written to JSON files and saved for later analysis. 
	/// JSON files are object-oriented and can be parsed clearly and easily. For more information: 
	/// <see cref="!:https://en.wikipedia.org/wiki/JSON">WIKI</see> and 
	///	AHref <a href="https://www.json.org/">JSON.ORG</a> <see href="https://en.wikipedia.org/wiki/JSON">HERE</see>
	/// </summary>
	public class ESJO : IEnumerator, IEnumerable
	{
		#region object properties

		// JSON object key
		private string key;
		public string Key { get { return key != null ? key : string.Empty; } }

		// string value
		private string strValue;
		public string StrValue { get { return strValue != null ? strValue : string.Empty; } }

		// double value
		private double dblValue;
		public double DblValue { get { return dblValue; } }

		// boolean value
		private bool boolValue;
		public bool BoolValue { get { return boolValue; } }

		// object array of doubles
		// e.g., for storing DVH object
		private List<Tuple<double, double>> tupLstValue;
		public List<Tuple<double, double>> TupLstValue { get { return tupLstValue != null ? tupLstValue : new List<Tuple<double, double>> { Tuple.Create(Double.NaN, Double.NaN) }; } }

		// object array of objects
		// e.g., for storing nested object layers
		private List<ESJO> jsonObjectsList;
		public List<ESJO> JsonObjectsList { get { return jsonObjectsList != null ? jsonObjectsList : new List<ESJO> { CreateESJO("emptyObjectKey", "emptyObjectValue") }; } }

		// output string in json format
		private string jsonString;
		public string JsonString { get { return jsonString != null ? jsonString : string.Empty; } }

		#endregion object properties

		#region object methods

		// Object Methods

		/// <summary>
		/// Null / Empty ESJO
		/// </summary>
		private ESJO()
		{
			key = null;
			strValue = null;
			dblValue = Double.NaN;
			boolValue = false;
			tupLstValue = null;
			jsonObjectsList = null;
			jsonString = null;
		}

		/// <summary>
		/// Creates an object with a value of Type String
		/// </summary>
		/// <param name="inputKey">ESJOs Key</param>
		/// <param name="value">ESJOs Value</param>
		/// <returns></returns>
		public static ESJO CreateESJO(string inputKey, string value)
		{
			ESJO esjo = new ESJO();
			esjo.key = inputKey;
			esjo.strValue = value;
			esjo.jsonString = string.Format("\"{0}\":\"{1}\"", esjo.key, esjo.strValue);

			return esjo;
		}

		/// <summary>
		/// Creates an object with a value of Type Double
		/// </summary>
		/// <param name="inputKey">ESJOs Key</param>
		/// <param name="value">ESJOs Value</param>
		/// <returns></returns>
		public static ESJO CreateESJO(string inputKey, double value)
		{
			ESJO esjo = new ESJO();
			esjo.key = inputKey;
			esjo.dblValue = value;
			esjo.jsonString = string.Format("\"{0}\":{1}", esjo.key, esjo.dblValue);

			return esjo;
		}

		/// <summary>
		/// Creates an object with a value of Type Bool
		/// </summary>
		/// <param name="inputKey">ESJOs Key</param>
		/// <param name="value">ESJOs Value</param>
		/// <returns></returns>
		public static ESJO CreateESJO(string inputKey, bool value)
		{
			ESJO esjo = new ESJO();
			esjo.key = inputKey;
			esjo.boolValue = value;
			esjo.jsonString = string.Format("\"{0}\":{1}", esjo.key, esjo.boolValue);

			return esjo;
		}

		/// <summary>
		/// Creates an object with its value being an array of doubles
		/// e.g., for storing a structure's DVH as an array
		/// </summary>
		/// <param name="inputKey">ESJOs Key</param>
		/// <param name="value">ESJOs Value</param>
		/// <returns></returns>
		public static ESJO CreateESJO(string inputKey, List<Tuple<double, double>> value)
		{
			ESJO esjo = new ESJO();
			esjo.key = inputKey;
			esjo.tupLstValue = value;
			esjo.jsonString = "\"" + esjo.key + "\":[";
			foreach (var tuple in esjo.tupLstValue)
			{
				esjo.jsonString += string.Format("[{0}], {1}],", tuple.Item1, tuple.Item2);
			}
			esjo.jsonString = esjo.jsonString.TrimEnd(',');
			esjo.jsonString += "]";

			return esjo;
		}

		/// <summary>
		/// Creates an object with its value being an array of other ESJOs - with pre-determined brackets [{}]
		/// e.g., for storing nested object layers
		/// </summary>
		/// <param name="inputKey">ESJOs Key</param>
		/// <param name="value">ESJOs Value</param>
		/// <returns></returns>
		public static ESJO CreateESJO(string inputKey, List<ESJO> value)
		{
			ESJO esjo = new ESJO();
			esjo.key = inputKey;
			esjo.jsonObjectsList = value;
			esjo.jsonString = "\"" + esjo.key + "\":[{";
			foreach (var jo in esjo.jsonObjectsList)
			{
				esjo.jsonString += jo.jsonString + ",";
			}
			esjo.jsonString = esjo.jsonString.TrimEnd(',');
			esjo.jsonString += "}]";

			return esjo;
		}

		/// <summary>
		/// Creates an object with its value being an array of other ESJOs - customizable brackets
		/// e.g., for storing nested object layers
		/// </summary>
		/// <param name="inputKey">ESJOs Key</param>
		/// <param name="value">ESJOs Value</param>
		/// <param name="openingBracket">Opening Bracket</param>
		/// <param name="closingBracket">Closing Bracket</param>
		/// <returns></returns>
		public static ESJO CreateESJO(string inputKey, List<ESJO> value, string openingBracket, string closingBracket)
		{
			ESJO esjo = new ESJO();
			esjo.key = inputKey;
			esjo.jsonObjectsList = value;
			esjo.jsonString = "\"" + esjo.key + "\":" + openingBracket;
			foreach (var jo in esjo.jsonObjectsList)
			{
				esjo.jsonString += jo.jsonString + ",";
			}
			esjo.jsonString = esjo.jsonString.TrimEnd(',');
			esjo.jsonString += closingBracket;

			return esjo;
		}


		/// <summary>
		/// Creates an ESJO made of other ESJOs - customizable brackets
		/// for nesting either a dictionary object or array object
		/// does not require a key
		/// </summary>
		/// <param name="listOfJsonObjects">List containing ESJOs</param>
		/// <param name="openingBracket">Opening Bracket</param>
		/// <param name="closingBracket">Closing Bracket</param>
		/// <returns></returns>
		public static ESJO CreateESJO(List<ESJO> listOfJsonObjects, string openingBracket, string closingBracket)
		{
			ESJO esjo = new ESJO();
			esjo.jsonString = openingBracket;
			esjo.jsonObjectsList = listOfJsonObjects;
			foreach (var jo in esjo.jsonObjectsList)
			{
				esjo.jsonString += jo.jsonString + ",";
			}
			esjo.jsonString = esjo.jsonString.TrimEnd(',');
			esjo.jsonString += closingBracket;

			return esjo;
		}

		#endregion object methods

		#region IEnumerator and IEnumerable Requirements
		/// <summary>
		/// position variable
		/// IEnumerator and IEnumerable require these methods.
		/// </summary>
		int position = -1;
		/// <summary>
		/// IEnumerator and IEnumerable require these methods.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)this;
		}

		/// <summary>
		/// IEnumerator and IEnumerable require these methods.
		/// IEnumerator
		/// </summary>
		/// <returns></returns>
		public bool MoveNext()
		{
			position++;
			return (position < JsonObjectsList.Count);
		}

		/// <summary>
		/// IEnumerator and IEnumerable require these methods.
		/// IEnumerable
		/// </summary>
		public void Reset()
		{ position = 0; }

		/// <summary>
		/// IEnumerator and IEnumerable require these methods.
		/// IEnumerable
		/// </summary>
		public object Current
		{
			get { return JsonObjectsList[position]; }
		}
		#endregion IEnumerator and IEnumerable Requirements
	}
}
