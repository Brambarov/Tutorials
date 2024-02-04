using Bricscad.ApplicationServices;
using System;
using Teigha.DatabaseServices;
using Teigha.Geometry;

namespace TestProject3.Classes
{
    public static class cBCFunctions
    {
        public static ObjectId GetObjectIdFromHandleString(string handleString)
        {
            ObjectId returnValue = ObjectId.Null;

            try
            {
                using (Database db = HostApplicationServices.WorkingDatabase)
                {
                    returnValue = db.GetObjectId(false, new Handle(Int64.Parse(handleString, System.Globalization.NumberStyles.AllowHexSpecifier)), 0);

                    if (returnValue.IsErased)
                    {
                        returnValue = ObjectId.Null;
                    }
                }
            }
            catch (Exception)
            {

            }

            return returnValue;
        }

        public static void CreateLayer(string name)
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (lt.Has(name) == false)
                {
                    lt.UpgradeOpen();

                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = name;
                    lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                }
                else
                {
                    LayerTableRecord ltr = tr.GetObject(lt[name], OpenMode.ForRead) as LayerTableRecord;

                    if (ltr.IsErased)
                    {
                        lt.UpgradeOpen();
                        ltr = new LayerTableRecord();
                        ltr.Name = name;
                        lt.Add(ltr);
                        tr.AddNewlyCreatedDBObject(ltr, true);
                    }
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// Create single line text and return the object
        /// </summary>
        /// <param name="location"></param>
        /// <param name="layer"></param>
        /// <param name="height"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static DBText PlaceSLText(Point3d location, string layer, double height, string contents)
        {
            DBText returnValue = null;

            CreateLayer(layer);

            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                DBText bcText = new DBText();
                bcText.SetDatabaseDefaults();
                bcText.TextString = contents;
                bcText.Position = location;
                bcText.Height = height;
                bcText.Layer = layer;

                btr.AppendEntity(bcText);
                tr.AddNewlyCreatedDBObject(bcText, true);

                returnValue = bcText;

                tr.Commit();
            }

            return returnValue;
        }
    }
}
