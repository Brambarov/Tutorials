using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using System;
using System.Windows.Forms;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using TestProject3.Classes;
using Application = Bricscad.ApplicationServices.Application;
using Point3dCollection = Teigha.Geometry.Point3dCollection;

namespace TestProject3.Forms
{
    public partial class ucPanelInformation : UserControl
    {
        public ucPanelInformation()
        {
            InitializeComponent();
            nudPricePM.Value = (decimal)cStart.PricePerM2;
        }

        private void lbParcels_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblInfo.Text = string.Empty;

            if (lbParcels.SelectedItem != null)
            {
                int parcelNumber = Convert.ToInt32(lbParcels.SelectedItem.ToString());

                foreach (cParcelObject po in cStart.AllParcels)
                {
                    if (po.Number == parcelNumber)
                    {
                        if (po.IsSold == 0)
                        {
                            lblInfo.Text = $"This parcel with an area of {po.Area:N2} m2 is for sale for {po.TotalPriceAsString}";
                        }
                        else
                        {
                            lblInfo.Text = $"Parcel {po.Number} is sold to {po.Name}";
                        }

                        using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                        {
                            using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                            {
                                Application.DocumentManager.MdiActiveDocument.Editor.Regen();

                                Polyline pline = tr.GetObject(po.Id, OpenMode.ForWrite) as Polyline;
                                pline.Highlight();
                                tr.Commit();

                                Bricscad.Internal.Utils.SetFocusToDwgView();
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void nudPricePM_ValueChanged(object sender, EventArgs e)
        {
            cStart.PricePerM2 = (double)nudPricePM.Value;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptStatus ps = PromptStatus.OK;

            while (ps is PromptStatus.OK)
            {
                PromptEntityResult per = ed.GetEntity("\nSelect a polyline");
                ps = per.Status;

                if (per.ObjectId != ObjectId.Null)
                {
                    bool selectedBefore = false;
                    int parcelNr = 0;

                    foreach (cParcelObject po in cStart.AllParcels)
                    {
                        if (po.Id == per.ObjectId)
                        {
                            selectedBefore = true;
                            parcelNr = po.Number;
                            break;
                        }
                    }

                    if (selectedBefore == false)
                    {
                        using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                        {
                            Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;

                            if (ent is Polyline)
                            {
                                cParcelObject po = new cParcelObject(per.ObjectId, cStart.ParcelNextNumber);
                                po.IsSold = 0;
                                cStart.AllParcels.Add(po);

                                lbParcels.Items.Add(po.Number.ToString());

                                cStart.ParcelNextNumber++;

                                ed.WriteMessage($"\nParcel {po.Number} added.");
                            }

                            tr.Commit();
                        }
                    }
                    else
                    {
                        ed.WriteMessage($"\nThis polyline is selected before as parcel {parcelNr}");
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (cStart.AllParcels.Count == 0)
            {
                MessageBox.Show("Select parcels first");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel file|*.csv";
            sfd.FilterIndex = 1;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamWriter writer = System.IO.File.CreateText(sfd.FileName);

                foreach (cParcelObject po in cStart.AllParcels)
                {
                    writer.WriteLine(po.Export());
                }

                writer.Close();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel|*.csv";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                cStart.AllParcels.Clear();
                lbParcels.Items.Clear();
                string line = string.Empty;
                System.IO.StreamReader reader = new System.IO.StreamReader(ofd.FileName);

                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] csvValues = line.Split(',');
                    ObjectId Id = cBCFunctions.GetObjectIdFromHandleString(csvValues[0]);
                    int number = Convert.ToInt32(csvValues[1]);
                    string name = csvValues[2];
                    int sold = Convert.ToInt32(csvValues[3]);

                    if (Id != ObjectId.Null)
                    {
                        cParcelObject po = new cParcelObject(Id, number);
                        po.Name = name;
                        po.IsSold = sold;
                        cStart.AllParcels.Add(po);
                        lbParcels.Items.Add(number);
                    }
                }

                reader.Close();
            }
        }

        private void lbParcels_DoubleClick(object sender, EventArgs e)
        {
            if (lbParcels.SelectedItems != null)
            {
                int parcelNr = Convert.ToInt32(lbParcels.SelectedItem.ToString());

                foreach (cParcelObject po in cStart.AllParcels)
                {
                    if (po.Number == parcelNr)
                    {
                        fParcelProperties form = new fParcelProperties(po);
                        break;
                    }
                }
            }
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    foreach (cParcelObject po in cStart.AllParcels)
                    {
                        Polyline pline = tr.GetObject(po.Id, OpenMode.ForRead) as Polyline;


                        Point3dCollection pts = new Point3dCollection();
                        pline.GetStretchPoints(pts);
                        double xTotal = 0.0d;
                        double yTotal = 0.0d;
                        foreach (Point3d pt in pts)
                        {
                            xTotal += pt.X;
                            yTotal += pt.Y;
                        }

                        Point3d ptCenterText = new Point3d(xTotal / pts.Count, yTotal / pts.Count, 0.0d);
                        string number = $"Nr. {po.Number}";

                        DBText text = cBCFunctions.PlaceSLText(ptCenterText, "ParcelNumbers", 5.0d, number);
                    }

                    tr.Commit();

                    Bricscad.Internal.Utils.SetFocusToDwgView();
                }
            }
        }
    }
}
