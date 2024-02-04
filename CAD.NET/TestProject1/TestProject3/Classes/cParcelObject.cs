using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;

namespace TestProject3.Classes
{
    public class cParcelObject
    {
        /// <summary>
        /// Create a new Parcel Object
        /// </summary>
        /// <param name="Id">ObjectId of Polyline</param>
        /// <param name="number">Unique number</param>
        public cParcelObject(ObjectId Id, int number)
        {
            this.Id = Id;
            this.Number = number;
            IsSold = 0;
            Name = string.Empty;
        }
        public ObjectId Id { get; private set; }
        public int Number { get; private set; }
        public string Name { get; set; }

        /// <summary>
        /// Indicates the status: 0 = for sale, 1 = sold
        /// </summary>
        public int IsSold { get; set; }
        public double Area
        {
            get
            {
                return GetPolylineArea();
            }
        }
        public double TotalPrice
        {
            get
            {
                return Area * cStart.PricePerM2;
            }
        }
        public string TotalPriceAsString
        {
            get
            {
                return $"$ {TotalPrice:0.00}";
            }
        }

        public string Export()
        {
            string name = Name.Replace(',', '-');
            string result = $"{Id.Handle},{Number:F0},{name},{IsSold:F0},{Area:F1},{TotalPrice:F2}";

            return result;
        }

        private double GetPolylineArea()
        {
            double returnValue = 0.0d;

            using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(Id, OpenMode.ForRead) as Entity;

                if (ent is Polyline)
                {
                    Polyline pl = ent as Polyline;
                    returnValue = pl.Area;
                }

                tr.Commit();
            }

            return returnValue;
        }
    }
}
