﻿using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;

namespace test_binding
{
    public partial class Form1 : Form
    {
        class myCustomCtrl
        {
            public DataGridView m_DGV;
            public Control m_ctrl;
            public bool m_bChanged = false;
            public int m_iRow;
            public int m_iCol;

            public myCustomCtrl(DataGridView dgv, Control ctrl)
            {
                m_DGV = dgv;
                m_ctrl = ctrl;
            }

            public virtual void show(Rectangle rec)
            {
                m_ctrl.Location = rec.Location;
                m_ctrl.Size = rec.Size;
                m_ctrl.Visible = true;

            }
            public virtual void hide() { m_ctrl.Visible = false; }
            public virtual bool isChanged() { return m_bChanged; }
            public virtual string getValue() { return ""; }
            public virtual void setValue(string text) { }
            public virtual Control getControl() { return m_ctrl; }
            public virtual void ctrl_ValueChanged(object sender, EventArgs e)
            {
                Debug.WriteLine("ctrl_ValueChanged");
                m_bChanged = true;
                m_DGV.NotifyCurrentCellDirty(true);
            }

            internal void reLocation()
            {
                Rectangle rec = m_DGV.GetCellDisplayRectangle(m_iCol, m_iRow, true);
                m_ctrl.Size = rec.Size;
                m_ctrl.Location = rec.Location;
            }
        }
        class myComboBox : myCustomCtrl
        {
            private ComboBox m_combo;

            //data table has single column
            public myComboBox(DataGridView dgv, DataTable tbl)
                : base(dgv, new ComboBox())
            {
                m_combo = (ComboBox)getControl();
                m_combo.DataSource = tbl;
                m_combo.DisplayMember = tbl.Columns[1].ColumnName;
                m_combo.SelectedValueChanged += ctrl_ValueChanged;
            }

            public override string getValue()
            {
                Debug.WriteLine("getValue: " + m_combo.Text);
                return m_combo.Text;
            }

            public override void setValue(string text)
            {
                Debug.WriteLine("setValue: " + text);
                m_combo.Text = text;
            }
        }
        class myDateTimePicker : myCustomCtrl
        {
            public DateTimePicker m_dtp;

            public myDateTimePicker(DataGridView dgv)
                : base(dgv, new DateTimePicker())
            {
                m_dtp = (DateTimePicker)getControl();
                m_dtp.Format = DateTimePickerFormat.Custom;
                m_dtp.CustomFormat = "yyyy-MM-dd";
                m_dtp.ValueChanged += ctrl_ValueChanged;
            }
            public override string getValue()
            {
                return m_dtp.Value.ToString("yyyy-MM-dd");
            }
            public override void setValue(string text)
            {
                DateTime dt;
                if (DateTime.TryParse(text, out dt))
                {
                    m_dtp.Value = dt;
                }
            }
        }
        
        class myDataGridView:DataGridView
        {
            lTableInfo m_tblInfo;
            myCustomCtrl m_customCtrl;

            public myDataGridView(lTableInfo tblInfo)
            {
                m_tblInfo = tblInfo;
            }

            protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
            {
                base.OnCellEndEdit(e);
                Debug.WriteLine("OnCellEndEdit");
                hideCustomCtrl();
            }
            protected override void OnCellClick(DataGridViewCellEventArgs e)
            {
                base.OnCellClick(e);
                Debug.WriteLine("OnCellClick");
                showCustomCtrl(e.ColumnIndex, e.RowIndex);
            }

            protected override void OnScroll(ScrollEventArgs e)
            {
                base.OnScroll(e);
                if (m_customCtrl != null)
                {
                    m_customCtrl.reLocation();
                }
            }

            lDataSync m_data;
            protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
            {
                base.OnEditingControlShowing(e);
                Debug.WriteLine("OnEditingControlShowing");
                do
                {
                    if (m_customCtrl != null) break;

                    m_data = m_tblInfo.m_cols[CurrentCell.ColumnIndex].m_lookupData;
                    if (m_data == null) break;

                    AutoCompleteStringCollection col = new AutoCompleteStringCollection();
                    DataTable tbl = m_data.m_dataSource;
                    foreach (DataRow row in tbl.Rows)
                    {
                        col.Add(row[1].ToString());
                    }
                    DataGridViewTextBoxEditingControl edt = (DataGridViewTextBoxEditingControl)e.Control;
                    edt.AutoCompleteMode = AutoCompleteMode.Suggest;
                    edt.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    edt.AutoCompleteCustomSource = col;

                    edt.Validated += Edt_Validated;
                } while (false);
            }

            private void Edt_Validated(object sender, EventArgs e)
            {
                TextBox edt = (TextBox)sender;
                Debug.WriteLine("Edt_Validated:" + edt.Text);
                string selectedValue = edt.Text;
                if (selectedValue != "" && !edt.AutoCompleteCustomSource.Contains(selectedValue))
                {
                    edt.AutoCompleteCustomSource.Add(selectedValue);
                    m_data.Add(selectedValue);
                }
                m_data = null;
                edt.Validated -= Edt_Validated;
            }

            public virtual void showCustomCtrl(int col, int row)
            {
                Debug.WriteLine("showDtp");
                if (m_tblInfo.m_cols[col].m_type == lTableInfo.lColInfo.lColType.dateTime) { 
                    m_customCtrl = new myDateTimePicker(this);
                }
                else if (m_tblInfo.m_cols[col].m_lookupData != null)
                {
                    m_customCtrl = new myComboBox(this, m_tblInfo.m_cols[col].m_lookupData.m_dataSource);
                }
                if (m_customCtrl != null)
                {
                    m_customCtrl.m_iRow = row;
                    m_customCtrl.m_iCol = col;
                    this.Controls.Add(m_customCtrl.getControl());
                    m_customCtrl.setValue(this.CurrentCell.Value.ToString());
                    Rectangle rec = this.GetCellDisplayRectangle(col, row, true);
                    m_customCtrl.show(rec);

                    //ActiveControl = m_dtp;
                    this.BeginEdit(true);
                }
            }
            public virtual void hideCustomCtrl()
            {
                if (m_customCtrl != null)
                {
                    Debug.WriteLine("hideDtp");
                    m_customCtrl.hide();

                    if (m_customCtrl.isChanged())
                    {
                        this.CurrentCell.Value = m_customCtrl.getValue();
                    }

                    this.Controls.Remove(m_customCtrl.getControl());
                    m_customCtrl = null;
                }
            }
        }
    }
}