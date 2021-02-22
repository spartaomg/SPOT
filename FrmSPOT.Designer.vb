<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmSPOT
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmSPOT))
        Me.BtnOptimize = New System.Windows.Forms.Button()
        Me.BtnLoadKla = New System.Windows.Forms.Button()
        Me.Pnl = New System.Windows.Forms.Panel()
        Me.PbxPic = New System.Windows.Forms.PictureBox()
        Me.PbxSH = New System.Windows.Forms.PictureBox()
        Me.PbxSL = New System.Windows.Forms.PictureBox()
        Me.PbxCR = New System.Windows.Forms.PictureBox()
        Me.PbxAC = New System.Windows.Forms.PictureBox()
        Me.BtnSettings = New System.Windows.Forms.Button()
        Me.TT = New System.Windows.Forms.ToolTip(Me.components)
        Me.STB = New System.Windows.Forms.StatusStrip()
        Me.TSSL = New System.Windows.Forms.ToolStripStatusLabel()
        Me.Pnl.SuspendLayout()
        CType(Me.PbxPic, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PbxSH, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PbxSL, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PbxCR, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PbxAC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.STB.SuspendLayout()
        Me.SuspendLayout()
        '
        'BtnOptimize
        '
        Me.BtnOptimize.Location = New System.Drawing.Point(534, 415)
        Me.BtnOptimize.Name = "BtnOptimize"
        Me.BtnOptimize.Size = New System.Drawing.Size(96, 32)
        Me.BtnOptimize.TabIndex = 0
        Me.BtnOptimize.Text = "Convert && Save"
        Me.TT.SetToolTip(Me.BtnOptimize, "Load, convert, optimize, and save an image file")
        Me.BtnOptimize.UseVisualStyleBackColor = True
        '
        'BtnLoadKla
        '
        Me.BtnLoadKla.Location = New System.Drawing.Point(432, 415)
        Me.BtnLoadKla.Name = "BtnLoadKla"
        Me.BtnLoadKla.Size = New System.Drawing.Size(96, 32)
        Me.BtnLoadKla.TabIndex = 2
        Me.BtnLoadKla.Text = "View Koala"
        Me.TT.SetToolTip(Me.BtnLoadKla, "Load and view a Koala image (no output generated)")
        Me.BtnLoadKla.UseVisualStyleBackColor = True
        '
        'Pnl
        '
        Me.Pnl.AutoScroll = True
        Me.Pnl.Controls.Add(Me.PbxPic)
        Me.Pnl.Controls.Add(Me.PbxSH)
        Me.Pnl.Controls.Add(Me.PbxSL)
        Me.Pnl.Controls.Add(Me.PbxCR)
        Me.Pnl.Controls.Add(Me.PbxAC)
        Me.Pnl.Location = New System.Drawing.Point(0, 0)
        Me.Pnl.Margin = New System.Windows.Forms.Padding(0)
        Me.Pnl.Name = "Pnl"
        Me.Pnl.Size = New System.Drawing.Size(640, 400)
        Me.Pnl.TabIndex = 9
        '
        'PbxPic
        '
        Me.PbxPic.BackColor = System.Drawing.Color.Black
        Me.PbxPic.Location = New System.Drawing.Point(0, 0)
        Me.PbxPic.Name = "PbxPic"
        Me.PbxPic.Size = New System.Drawing.Size(640, 400)
        Me.PbxPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PbxPic.TabIndex = 12
        Me.PbxPic.TabStop = False
        Me.TT.SetToolTip(Me.PbxPic, "Press left mouse button to see original image. Press keys 1-4 to see C64 color di" &
        "stribution and 5 to see converted image")
        '
        'PbxSH
        '
        Me.PbxSH.BackColor = System.Drawing.Color.Black
        Me.PbxSH.Location = New System.Drawing.Point(0, 0)
        Me.PbxSH.Name = "PbxSH"
        Me.PbxSH.Size = New System.Drawing.Size(640, 400)
        Me.PbxSH.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PbxSH.TabIndex = 11
        Me.PbxSH.TabStop = False
        '
        'PbxSL
        '
        Me.PbxSL.BackColor = System.Drawing.Color.Black
        Me.PbxSL.Location = New System.Drawing.Point(0, 0)
        Me.PbxSL.Name = "PbxSL"
        Me.PbxSL.Size = New System.Drawing.Size(640, 400)
        Me.PbxSL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PbxSL.TabIndex = 10
        Me.PbxSL.TabStop = False
        '
        'PbxCR
        '
        Me.PbxCR.BackColor = System.Drawing.Color.Black
        Me.PbxCR.Location = New System.Drawing.Point(0, 0)
        Me.PbxCR.Name = "PbxCR"
        Me.PbxCR.Size = New System.Drawing.Size(640, 400)
        Me.PbxCR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PbxCR.TabIndex = 9
        Me.PbxCR.TabStop = False
        '
        'PbxAC
        '
        Me.PbxAC.BackColor = System.Drawing.Color.Black
        Me.PbxAC.Location = New System.Drawing.Point(0, 0)
        Me.PbxAC.Name = "PbxAC"
        Me.PbxAC.Size = New System.Drawing.Size(640, 400)
        Me.PbxAC.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PbxAC.TabIndex = 13
        Me.PbxAC.TabStop = False
        Me.TT.SetToolTip(Me.PbxAC, "Press Left mouse button to see original image. Press keys 1-4 to see C64 color di" &
        "stribution.")
        '
        'BtnSettings
        '
        Me.BtnSettings.Location = New System.Drawing.Point(12, 415)
        Me.BtnSettings.Name = "BtnSettings"
        Me.BtnSettings.Size = New System.Drawing.Size(96, 32)
        Me.BtnSettings.TabIndex = 1
        Me.BtnSettings.Text = "Settings"
        Me.TT.SetToolTip(Me.BtnSettings, "Select output file formats")
        Me.BtnSettings.UseVisualStyleBackColor = True
        '
        'STB
        '
        Me.STB.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TSSL})
        Me.STB.Location = New System.Drawing.Point(0, 461)
        Me.STB.Name = "STB"
        Me.STB.Size = New System.Drawing.Size(640, 22)
        Me.STB.TabIndex = 10
        Me.STB.Text = "TEST"
        '
        'TSSL
        '
        Me.TSSL.Name = "TSSL"
        Me.TSSL.Size = New System.Drawing.Size(0, 17)
        '
        'FrmSPOT
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(640, 483)
        Me.Controls.Add(Me.STB)
        Me.Controls.Add(Me.BtnSettings)
        Me.Controls.Add(Me.Pnl)
        Me.Controls.Add(Me.BtnLoadKla)
        Me.Controls.Add(Me.BtnOptimize)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.MinimumSize = New System.Drawing.Size(656, 522)
        Me.Name = "FrmSPOT"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SPOT - Sparta's Picture Optimizing Tool for the Commodore 64"
        Me.Pnl.ResumeLayout(False)
        CType(Me.PbxPic, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PbxSH, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PbxSL, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PbxCR, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PbxAC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.STB.ResumeLayout(False)
        Me.STB.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents BtnOptimize As Button
    Friend WithEvents BtnLoadKla As Button
    Friend WithEvents Pnl As Panel
    Friend WithEvents PbxPic As PictureBox
    Friend WithEvents PbxSH As PictureBox
    Friend WithEvents PbxSL As PictureBox
    Friend WithEvents PbxCR As PictureBox
    Friend WithEvents BtnSettings As Button
    Friend WithEvents TT As ToolTip
    Friend WithEvents STB As StatusStrip
    Friend WithEvents TSSL As ToolStripStatusLabel
    Friend WithEvents PbxAC As PictureBox
End Class
