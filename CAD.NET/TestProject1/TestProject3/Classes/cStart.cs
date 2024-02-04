using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Bricscad.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using Teigha.Runtime;

namespace TestProject3.Classes
{
    public class cStart
    {
        public static PaletteSet ps = null;
        public static double PricePerM2 = 100.0d;
        public static int ParcelNextNumber = 1;
        public static List<cParcelObject> AllParcels = new List<cParcelObject>();

        [CommandMethod("Parcels")]
        public void cmdParcels()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                if (ps is null)
                {
                    ps = new PaletteSet("OwnerShip", new Guid("{8E3487CD-7C51-41B8-9A54-E38ABC666C95}"));
                    ps.Add("OverView", new Forms.ucPanelInformation());
                }

                ps.Visible = true;
                ps.MinimumSize = new Size(300, 600);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError: {ex.Message}");
            }
        }
    }
}
