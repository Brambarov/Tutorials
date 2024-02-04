using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Runtime;

namespace TestProject2
{
    public class BricsCADCommands
    {
        [CommandMethod("SetFactor")]
        public void cmdSetFactor()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptEntityResult per = ed.GetEntity("\nSelect object");

            if (per.ObjectId != ObjectId.Null)
            {
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;

                    if (ent is BlockReference)
                    {
                        BlockReference block = ent as BlockReference;

                        AttributeReference att = new AttributeReference();

                        foreach (var attId in block.AttributeCollection)
                        {
                            att = tr.GetObject((ObjectId)attId, OpenMode.ForWrite) as AttributeReference;

                            if (att.Tag == "Faktor")
                            {
                                ed.WriteMessage($"\nCurrent object factor: {att.TextString}");

                                PromptIntegerOptions getIntegerResult = new PromptIntegerOptions("\nEnter new factor: ")
                                {
                                    AllowNegative = false,
                                    AllowZero = false
                                };

                                PromptIntegerResult result = ed.GetInteger(getIntegerResult);

                                if (result.Status is PromptStatus.OK)
                                {
                                    att.TextString = result.Value.ToString();
                                }

                                break;
                            }
                        }

                        ed.WriteMessage($"New object factor: {att.TextString}");

                    }
                    else
                    {
                        ed.WriteMessage("\nSelected object is not a block!");
                    }

                    tr.Commit();
                }
            }
        }
    }
}
