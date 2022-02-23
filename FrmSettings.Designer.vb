<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmSettings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmSettings))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.ChkAutoSave = New System.Windows.Forms.CheckBox()
        Me.ChkJpg = New System.Windows.Forms.CheckBox()
        Me.ChkBmp = New System.Windows.Forms.CheckBox()
        Me.ChkPng = New System.Windows.Forms.CheckBox()
        Me.ChkRmp = New System.Windows.Forms.CheckBox()
        Me.ChkC2 = New System.Windows.Forms.CheckBox()
        Me.ChkScr = New System.Windows.Forms.CheckBox()
        Me.ChkCol = New System.Windows.Forms.CheckBox()
        Me.ChkMap = New System.Windows.Forms.CheckBox()
        Me.ChkKla = New System.Windows.Forms.CheckBox()
        Me.LblOutputFormat = New System.Windows.Forms.Label()
        Me.BtnClose = New System.Windows.Forms.Button()
        Me.ChkBgc = New System.Windows.Forms.CheckBox()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.ChkBgc)
        Me.Panel1.Controls.Add(Me.ChkAutoSave)
        Me.Panel1.Controls.Add(Me.ChkJpg)
        Me.Panel1.Controls.Add(Me.ChkBmp)
        Me.Panel1.Controls.Add(Me.ChkPng)
        Me.Panel1.Controls.Add(Me.ChkRmp)
        Me.Panel1.Controls.Add(Me.ChkC2)
        Me.Panel1.Controls.Add(Me.ChkScr)
        Me.Panel1.Controls.Add(Me.ChkCol)
        Me.Panel1.Controls.Add(Me.ChkMap)
        Me.Panel1.Controls.Add(Me.ChkKla)
        Me.Panel1.Controls.Add(Me.LblOutputFormat)
        Me.Panel1.Location = New System.Drawing.Point(13, 13)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(216, 301)
        Me.Panel1.TabIndex = 0
        '
        'ChkAutoSave
        '
        Me.ChkAutoSave.AutoSize = True
        Me.ChkAutoSave.Location = New System.Drawing.Point(10, 278)
        Me.ChkAutoSave.Name = "ChkAutoSave"
        Me.ChkAutoSave.Size = New System.Drawing.Size(161, 17)
        Me.ChkAutoSave.TabIndex = 10
        Me.ChkAutoSave.Text = "Autosave to SPOT subfolder"
        Me.ChkAutoSave.UseVisualStyleBackColor = True
        '
        'ChkJpg
        '
        Me.ChkJpg.AutoSize = True
        Me.ChkJpg.Location = New System.Drawing.Point(10, 237)
        Me.ChkJpg.Name = "ChkJpg"
        Me.ChkJpg.Size = New System.Drawing.Size(83, 17)
        Me.ChkJpg.TabIndex = 9
        Me.ChkJpg.Text = "JPEG (*.jpg)"
        Me.ChkJpg.UseVisualStyleBackColor = True
        '
        'ChkBmp
        '
        Me.ChkBmp.AutoSize = True
        Me.ChkBmp.Location = New System.Drawing.Point(10, 214)
        Me.ChkBmp.Name = "ChkBmp"
        Me.ChkBmp.Size = New System.Drawing.Size(85, 17)
        Me.ChkBmp.TabIndex = 8
        Me.ChkBmp.Text = "BMP (*.bmp)"
        Me.ChkBmp.UseVisualStyleBackColor = True
        '
        'ChkPng
        '
        Me.ChkPng.AutoSize = True
        Me.ChkPng.Location = New System.Drawing.Point(10, 191)
        Me.ChkPng.Name = "ChkPng"
        Me.ChkPng.Size = New System.Drawing.Size(83, 17)
        Me.ChkPng.TabIndex = 7
        Me.ChkPng.Text = "PNG (*.png)"
        Me.ChkPng.UseVisualStyleBackColor = True
        '
        'ChkRmp
        '
        Me.ChkRmp.AutoSize = True
        Me.ChkRmp.Location = New System.Drawing.Point(10, 168)
        Me.ChkRmp.Name = "ChkRmp"
        Me.ChkRmp.Size = New System.Drawing.Size(142, 17)
        Me.ChkRmp.TabIndex = 6
        Me.ChkRmp.Text = "Optimized bitmap (*.obm)"
        Me.ChkRmp.UseVisualStyleBackColor = True
        '
        'ChkC2
        '
        Me.ChkC2.AutoSize = True
        Me.ChkC2.Location = New System.Drawing.Point(10, 145)
        Me.ChkC2.Name = "ChkC2"
        Me.ChkC2.Size = New System.Drawing.Size(168, 17)
        Me.ChkC2.TabIndex = 5
        Me.ChkC2.Text = "Compressed color RAM (*.ccr)"
        Me.ChkC2.UseVisualStyleBackColor = True
        '
        'ChkScr
        '
        Me.ChkScr.AutoSize = True
        Me.ChkScr.Location = New System.Drawing.Point(10, 99)
        Me.ChkScr.Name = "ChkScr"
        Me.ChkScr.Size = New System.Drawing.Size(117, 17)
        Me.ChkScr.TabIndex = 4
        Me.ChkScr.Text = "Screen RAM (*.scr)"
        Me.ChkScr.UseVisualStyleBackColor = True
        '
        'ChkCol
        '
        Me.ChkCol.AutoSize = True
        Me.ChkCol.Location = New System.Drawing.Point(10, 76)
        Me.ChkCol.Name = "ChkCol"
        Me.ChkCol.Size = New System.Drawing.Size(107, 17)
        Me.ChkCol.TabIndex = 3
        Me.ChkCol.Text = "Color RAM (*.col)"
        Me.ChkCol.UseVisualStyleBackColor = True
        '
        'ChkMap
        '
        Me.ChkMap.AutoSize = True
        Me.ChkMap.Location = New System.Drawing.Point(10, 53)
        Me.ChkMap.Name = "ChkMap"
        Me.ChkMap.Size = New System.Drawing.Size(118, 17)
        Me.ChkMap.TabIndex = 2
        Me.ChkMap.Text = "Bitmap data (*.map)"
        Me.ChkMap.UseVisualStyleBackColor = True
        '
        'ChkKla
        '
        Me.ChkKla.AutoSize = True
        Me.ChkKla.Location = New System.Drawing.Point(10, 30)
        Me.ChkKla.Name = "ChkKla"
        Me.ChkKla.Size = New System.Drawing.Size(83, 17)
        Me.ChkKla.TabIndex = 1
        Me.ChkKla.Text = "Koala (*.kla)"
        Me.ChkKla.UseVisualStyleBackColor = True
        '
        'LblOutputFormat
        '
        Me.LblOutputFormat.AutoSize = True
        Me.LblOutputFormat.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblOutputFormat.Location = New System.Drawing.Point(3, 3)
        Me.LblOutputFormat.Name = "LblOutputFormat"
        Me.LblOutputFormat.Size = New System.Drawing.Size(132, 13)
        Me.LblOutputFormat.TabIndex = 0
        Me.LblOutputFormat.Text = "Select output formats:"
        '
        'BtnClose
        '
        Me.BtnClose.Location = New System.Drawing.Point(154, 325)
        Me.BtnClose.Name = "BtnClose"
        Me.BtnClose.Size = New System.Drawing.Size(75, 25)
        Me.BtnClose.TabIndex = 5
        Me.BtnClose.Text = "Close"
        Me.BtnClose.UseVisualStyleBackColor = True
        '
        'ChkBgc
        '
        Me.ChkBgc.AutoSize = True
        Me.ChkBgc.Location = New System.Drawing.Point(10, 122)
        Me.ChkBgc.Name = "ChkBgc"
        Me.ChkBgc.Size = New System.Drawing.Size(144, 17)
        Me.ChkBgc.TabIndex = 11
        Me.ChkBgc.Text = "Background color (*.bgc)"
        Me.ChkBgc.UseVisualStyleBackColor = True
        '
        'FrmSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(240, 359)
        Me.Controls.Add(Me.BtnClose)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmSettings"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Settings"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents ChkPng As CheckBox
    Friend WithEvents ChkRmp As CheckBox
    Friend WithEvents ChkC2 As CheckBox
    Friend WithEvents ChkScr As CheckBox
    Friend WithEvents ChkCol As CheckBox
    Friend WithEvents ChkMap As CheckBox
    Friend WithEvents ChkKla As CheckBox
    Friend WithEvents LblOutputFormat As Label
    Friend WithEvents ChkJpg As CheckBox
    Friend WithEvents ChkBmp As CheckBox
    Friend WithEvents BtnClose As Button
    Friend WithEvents ChkAutoSave As CheckBox
    Friend WithEvents ChkBgc As CheckBox
End Class
