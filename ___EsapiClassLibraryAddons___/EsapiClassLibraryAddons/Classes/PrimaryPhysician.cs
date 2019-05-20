namespace VMS.TPS
{
	using System;
	/// <summary>
	/// PrimaryPhysician Class -- Used for creating a tailored Primary Physician Object
	/// </summary>
	public class PrimaryPhysician
	{
        private string id;
        public string Id
        {
            get { return id; }
            set { this.id = value; }
        }
        private string name;
		public string Name
		{
			get { return name; }
			set { this.name = value; }
		}

        public PrimaryPhysician()
        {
            id = string.Empty;
            name = string.Empty;
        }
		/// <summary>
		/// Create PrimaryPhysician Object
		/// </summary>
		/// <param name="physicianId"></param>
		/// <returns>PrimaryPhysician Object</returns>
		public static PrimaryPhysician GetPrimaryPhysician(string physicianId)
		{
			PrimaryPhysician pp = new PrimaryPhysician();

      pp.id = physicianId;
			pp.name = VMS.TPS.GetPrimary.Physician(physicianId); // reference to EsapiAddons.dll

			return pp;
		}
	}
}
