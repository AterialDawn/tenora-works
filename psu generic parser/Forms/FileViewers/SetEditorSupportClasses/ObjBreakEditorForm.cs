using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PSULib.FileClasses.Missions.SetFile;

namespace psu_generic_parser.Forms.FileViewers.SetEditorSupportClasses
{
    public partial class ObjBreakEditorForm : Form
    {
        BindingList<EditableObjectEntry> entriesList;
        List<ObjectEntry> objects;
        public ObjBreakEditorForm(string mapId, List<ObjectEntry> listOfObjects)
        {
            InitializeComponent();

            objects = listOfObjects;

            mapNumberLabel.Text = $"Editing for Map : {mapId}";

            entriesList = new BindingList<EditableObjectEntry>();

            foreach (var obj in listOfObjects)
            {
                entriesList.Add(new EditableObjectEntry { LST = obj.metadata[0x2], IND = obj.metadata[0x3] });
            }

            var source = new BindingSource(entriesList, null);

            dataGrid.DataSource = source;

            dataGrid.Columns[0].DisplayIndex = 1;
            dataGrid.Columns[1].DisplayIndex = 0;
            dataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            DialogResult = DialogResult.Cancel;
        }

        class EditableObjectEntry
        {
            public int IND { get; set; }
            public int LST { get; set; }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            //commit changes to listOfObjects
            DialogResult = DialogResult.OK;

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].metadata[0x2] = (byte)entriesList[i].LST;
                objects[i].metadata[0x3] = (byte)entriesList[i].IND;
            }
            Close();
        }

        private void dataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                var textContent = Clipboard.GetText();
                var imported = textContent.Split('\n').Select(s => s.Trim('\r')).Where(s => s.Split('\t').Length == 2).Select(s => { var split = s.Split('\t'); return new EditableObjectEntry { LST = int.Parse(split[0]), IND = int.Parse(split[1]) }; }).ToArray(); //lmao

                if (imported.Length == entriesList.Count)
                {
                    entriesList.Clear();
                    foreach (var item in imported) entriesList.Add(item);

                    var source = new BindingSource(entriesList, null);

                    dataGrid.DataSource = source;
                }
                else
                {
                    MessageBox.Show($"Incorrect number of elements. Expected {entriesList.Count}, got {imported.Length}");
                }
            }
        }
    }
}
