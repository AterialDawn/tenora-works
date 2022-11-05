using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PSULib.FileClasses.Characters;

namespace psu_generic_parser
{
    public partial class PartsInfoViewer : UserControl
    {
        PartsInfoFile internalFile;
        int firstUnusedPartNumber = 0;
        public PartsInfoViewer(PartsInfoFile toImport)
        {
            InitializeComponent();
            internalFile = toImport;
            dataGridView1.Columns[0].DefaultCellStyle.Format = "X08";
            dataGridView1.Columns[2].DefaultCellStyle.Format = "X08";
            dataGridView1.Rows.Add(internalFile.parts.Length);
            for (int i = 0; i < internalFile.parts.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(internalFile.parts[i].partNumber);
                Array.Reverse(temp);
                dataGridView1[0, i].Value = BitConverter.ToInt32(temp, 0);
                dataGridView1[0, i].ReadOnly = false;
                dataGridView1[1, i].Value = internalFile.parts[i].fileNumber;
                dataGridView1[1, i].ReadOnly = false;
                dataGridView1[2, i].Value = internalFile.parts[i].unknownFlags;
                dataGridView1[2, i].ReadOnly = false;
                dataGridView1[3, i].Value = internalFile.parts[i].fileName;
                dataGridView1[3, i].ReadOnly = false;

                if (internalFile.parts[i].fileNumber >= firstUnusedPartNumber)
                {
                    firstUnusedPartNumber = internalFile.parts[i].fileNumber + 1;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                internalFile.importFile(openFileDialog1.OpenFile());
                dataGridView1.Rows.Clear();
                dataGridView1.Rows.Add(internalFile.parts.Length);
                for (int i = 0; i < internalFile.parts.Length; i++)
                {
                    byte[] temp = BitConverter.GetBytes(internalFile.parts[i].partNumber);
                    Array.Reverse(temp);
                    dataGridView1[0, i].Value = BitConverter.ToInt32(temp, 0);
                    dataGridView1[0, i].ReadOnly = false;
                    dataGridView1[1, i].Value = internalFile.parts[i].fileNumber;
                    dataGridView1[1, i].ReadOnly = false;
                    dataGridView1[2, i].Value = internalFile.parts[i].unknownFlags;
                    dataGridView1[2, i].ReadOnly = false;
                    dataGridView1[3, i].Value = internalFile.parts[i].fileName;
                    dataGridView1[3, i].ReadOnly = false;
                }
            }
        }

        private void addItemsButton_Click(object sender, EventArgs e)
        {
            var newPartsInfo = new PartsInfoFile.partsInfo();
            newPartsInfo.partNumber = BitConverter.ToInt32(BitConverter.GetBytes(-559038737).Reverse().ToArray(),0);
            newPartsInfo.fileNumber = firstUnusedPartNumber++;
            newPartsInfo.unknownFlags = 0x0000001D;
            newPartsInfo.fileName = "CHANGE_ME.nbl";
            var newList = new List<PartsInfoFile.partsInfo>();
            newList.AddRange(internalFile.parts);

            int rowNumber = 0;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                rowNumber = dataGridView1.Rows.Add();
                var rowData = dataGridView1.Rows[rowNumber];
                dataGridView1.Rows.RemoveAt(rowNumber);
                dataGridView1.Rows.Insert(selectedIndex, rowData);

                rowNumber = selectedIndex;
                newList.Insert(selectedIndex, newPartsInfo);
            }
            else
            {
                rowNumber = dataGridView1.Rows.Add();
                newList.Add(newPartsInfo);
            }

            internalFile.parts = newList.ToArray();
            dataGridView1[0, rowNumber].Value = -559038737;
            dataGridView1[0, rowNumber].ReadOnly = false;
            dataGridView1[1, rowNumber].Value = newPartsInfo.fileNumber;
            dataGridView1[1, rowNumber].ReadOnly = false;
            dataGridView1[2, rowNumber].Value = newPartsInfo.unknownFlags;
            dataGridView1[2, rowNumber].ReadOnly = false;
            dataGridView1[3, rowNumber].Value = newPartsInfo.fileName;
            dataGridView1[3, rowNumber].ReadOnly = false;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    byte[] temp = BitConverter.GetBytes(int.Parse((string)dataGridView1[e.ColumnIndex, e.RowIndex].Value, System.Globalization.NumberStyles.HexNumber));
                    Array.Reverse(temp);
                    internalFile.parts[e.RowIndex].partNumber = BitConverter.ToInt32(temp, 0);
                    break;
                case 1:
                    internalFile.parts[e.RowIndex].fileNumber = int.Parse((string)dataGridView1[e.ColumnIndex, e.RowIndex].Value);
                    break;
                case 2:
                    internalFile.parts[e.RowIndex].unknownFlags = int.Parse((string)dataGridView1[e.ColumnIndex, e.RowIndex].Value, System.Globalization.NumberStyles.HexNumber);
                    break;
                case 3:
                    internalFile.parts[e.RowIndex].fileName = (string)dataGridView1[e.ColumnIndex, e.RowIndex].Value;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Cell Edit Index {e.RowIndex}:{e.ColumnIndex}");
            }
        }

        private void deleteItemButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                dataGridView1.Rows.RemoveAt(selectedIndex);
                List<PartsInfoFile.partsInfo> list = new List<PartsInfoFile.partsInfo>();
                list.AddRange(internalFile.parts);
                list.Remove(internalFile.parts[selectedIndex]);
                internalFile.parts = list.ToArray();
            }
        }
    }
}
