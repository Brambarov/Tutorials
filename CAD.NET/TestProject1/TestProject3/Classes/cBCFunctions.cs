using Bricscad.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Application = Bricscad.ApplicationServices.Application;

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

        /// <summary>
        /// Create a circle and return the object
        /// </summary>
        /// <param name="location"></param>
        /// <param name="radius"></param>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Circle PlaceCircle(Point3d location, double radius, string layer, int color)
        {
            Circle returnValue = new Circle();

            CreateLayer(layer);

            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Circle bcCircle = new Circle(location, Vector3d.ZAxis, radius);
                bcCircle.SetDatabaseDefaults();
                bcCircle.Layer = layer;
                bcCircle.ColorIndex = color;

                btr.AppendEntity(bcCircle);
                tr.AddNewlyCreatedDBObject(bcCircle, true);
                returnValue = bcCircle;

                tr.Commit();
            }

            return returnValue;
        }

        /// <summary>
        /// Save a textstring in the ExtensionDictionary of an object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetObjXRecordText(ObjectId id, string key, string value)
        {
            using (ResultBuffer resBuf = new ResultBuffer(new TypedValue((int)DxfCode.Text, value)))
            {
                SetXRecordInObject(id, key, resBuf);
            }
        }

        /// <summary>
        /// Save an integer in the ExtensionDictionary of an object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetObjXRecordInt32(ObjectId id, string key, int value)
        {
            using (ResultBuffer resBuf = new ResultBuffer(new TypedValue((int)DxfCode.Int32, value)))
            {
                SetXRecordInObject(id, key, resBuf);
            }
        }

        /// <summary>
        /// Read a string from the ExtensionDictionary of an object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetObjXRecordText(ObjectId id, string key)
        {
            string returnValue = string.Empty;

            List<object> records = GetXRecordFromObject(id, key);

            if (records.Count > 0)
            {
                returnValue = Convert.ToString(records[records.Count - 1]);
            }

            return returnValue;
        }

        /// <summary>
        /// Read an integer from the ExtensionDictionary of an object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetObjXRecordInt32(ObjectId id, string key)
        {
            int returnValue = -1;

            List<object> records = GetXRecordFromObject(id, key);

            if (records.Count > 0)
            {
                returnValue = Convert.ToInt32(records[records.Count - 1]);
            }

            return returnValue;
        }

        private static void SetXRecordInObject(ObjectId id, string key, ResultBuffer rb)
        {
            try
            {
                using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    Database db = Application.DocumentManager.MdiActiveDocument.Database;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        using (Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity)
                        {
                            if (ent != null)
                            {
                                if (ent.ExtensionDictionary == ObjectId.Null)
                                {
                                    ent.CreateExtensionDictionary();
                                }

                                using (DBDictionary xDict = tr.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite) as DBDictionary)
                                {
                                    using (Xrecord xRec = new Xrecord())
                                    {
                                        xRec.Data = rb;

                                        try
                                        {
                                            xDict.Remove(key);
                                        }
                                        catch (Exception)
                                        {

                                        }

                                        xDict.SetAt(key, xRec);

                                        tr.AddNewlyCreatedDBObject(xRec, true);
                                    }
                                }
                            }
                        }

                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static List<object> GetXRecordFromObject(ObjectId id, string key)
        {
            List<object> returnValue = new List<object>();

            try
            {
                using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    Database db = Application.DocumentManager.MdiActiveDocument.Database;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        using (Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity)
                        {
                            if (ent.ExtensionDictionary == ObjectId.Null)
                            {
                                return returnValue;
                            }

                            try
                            {
                                using (DBDictionary xDict = tr.GetObject(ent.ExtensionDictionary, OpenMode.ForRead, false) as DBDictionary)
                                {
                                    using (Xrecord xRec = tr.GetObject(xDict.GetAt(key), OpenMode.ForRead, false) as Xrecord)
                                    {
                                        TypedValue[] xRecData = xRec.Data.AsArray();

                                        foreach (TypedValue tv in xRecData)
                                        {
                                            returnValue.Add(tv.Value);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }

                            tr.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return returnValue;
        }
    }
}
