Public Class FilePropDemo
    Inherits System.Windows.Forms.Form

    ' DSOFile.OleDocumentPropertiesClass
    Private m_oDocument As DSOFile.OleDocumentPropertiesClass
    Private m_fOpenedReadOnly As Boolean

    Const c_strFilter As String = "Office Document Files|*.doc;*.docx;*.docm;*.dot;*.dotx;*.xls;*.xlsx;*.xlsm;*.xlsb;*.xla;*.xlam;*.ppt;*.pptx;*.pptm;*.vsd|All Files (*.*)|*.*"

#Region "Windows Form Designer Code"
    Private components As System.ComponentModel.IContainer
    ' Main Form controls...
    Friend WithEvents cmdOpen As System.Windows.Forms.Button
    Friend picIcon As System.Windows.Forms.PictureBox
    Friend WithEvents PropTabs As System.Windows.Forms.TabControl
    Friend SummaryTab As System.Windows.Forms.TabPage
    Friend StatTab As System.Windows.Forms.TabPage
    Friend CustomTab As System.Windows.Forms.TabPage
    ' Summary Page Controls...
    Friend WithEvents txtTitle As System.Windows.Forms.TextBox
    Friend WithEvents txtAuthor As System.Windows.Forms.TextBox
    Friend WithEvents txtSubject As System.Windows.Forms.TextBox
    Friend WithEvents txtCompany As System.Windows.Forms.TextBox
    Friend WithEvents txtComments As System.Windows.Forms.TextBox
    Friend lbFilePath As System.Windows.Forms.Label
    Friend lbSumPath As System.Windows.Forms.Label
    Friend lbSumIntro As System.Windows.Forms.Label
    Friend lbSumComments As System.Windows.Forms.Label
    Friend lbSumCompany As System.Windows.Forms.Label
    Friend lbSumSubject As System.Windows.Forms.Label
    Friend lbSumAuthor As System.Windows.Forms.Label
    Friend lbSumTitle As System.Windows.Forms.Label
    ' Statistics Page Controls...
    Friend WithEvents StatListView As System.Windows.Forms.ListView
    Friend StatNameCol As System.Windows.Forms.ColumnHeader
    Friend StatValCol As System.Windows.Forms.ColumnHeader
    Friend lbStatIntro As System.Windows.Forms.Label
    Friend lbStatNote As System.Windows.Forms.Label
    ' Custom Page Controls...
    Friend WithEvents CustListView As System.Windows.Forms.ListView
    Friend CustNameCol As System.Windows.Forms.ColumnHeader
    Friend CustValueCol As System.Windows.Forms.ColumnHeader
    Friend CustTypeCol As System.Windows.Forms.ColumnHeader
    Friend WithEvents cmdAdd As System.Windows.Forms.Button
    Friend WithEvents cmdRemove As System.Windows.Forms.Button
    Friend txtCustName As System.Windows.Forms.TextBox
    Friend txtCustValue As System.Windows.Forms.TextBox
    Friend cboxCustType As System.Windows.Forms.ComboBox
    Friend lbCustType As System.Windows.Forms.Label
    Friend lbCustValue As System.Windows.Forms.Label
    Friend lbCustName As System.Windows.Forms.Label
    Friend lbCustNote As System.Windows.Forms.Label
    Friend lbCustIntro As System.Windows.Forms.Label

    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' Don't edit this function, the desginer will overwrite this automatically.
    ' Custom settings should be applied in separate function...
    Friend WithEvents lbFileName As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.cmdOpen = New System.Windows.Forms.Button
        Me.lbFileName = New System.Windows.Forms.Label
        Me.picIcon = New System.Windows.Forms.PictureBox
        Me.PropTabs = New System.Windows.Forms.TabControl
        Me.SummaryTab = New System.Windows.Forms.TabPage
        Me.lbFilePath = New System.Windows.Forms.Label
        Me.lbSumPath = New System.Windows.Forms.Label
        Me.lbSumIntro = New System.Windows.Forms.Label
        Me.txtComments = New System.Windows.Forms.TextBox
        Me.lbSumComments = New System.Windows.Forms.Label
        Me.txtCompany = New System.Windows.Forms.TextBox
        Me.lbSumCompany = New System.Windows.Forms.Label
        Me.txtSubject = New System.Windows.Forms.TextBox
        Me.lbSumSubject = New System.Windows.Forms.Label
        Me.txtAuthor = New System.Windows.Forms.TextBox
        Me.lbSumAuthor = New System.Windows.Forms.Label
        Me.txtTitle = New System.Windows.Forms.TextBox
        Me.lbSumTitle = New System.Windows.Forms.Label
        Me.StatTab = New System.Windows.Forms.TabPage
        Me.lbStatNote = New System.Windows.Forms.Label
        Me.lbStatIntro = New System.Windows.Forms.Label
        Me.StatListView = New System.Windows.Forms.ListView
        Me.StatNameCol = New System.Windows.Forms.ColumnHeader
        Me.StatValCol = New System.Windows.Forms.ColumnHeader
        Me.CustomTab = New System.Windows.Forms.TabPage
        Me.cboxCustType = New System.Windows.Forms.ComboBox
        Me.lbCustType = New System.Windows.Forms.Label
        Me.lbCustValue = New System.Windows.Forms.Label
        Me.lbCustName = New System.Windows.Forms.Label
        Me.lbCustNote = New System.Windows.Forms.Label
        Me.txtCustValue = New System.Windows.Forms.TextBox
        Me.txtCustName = New System.Windows.Forms.TextBox
        Me.cmdRemove = New System.Windows.Forms.Button
        Me.cmdAdd = New System.Windows.Forms.Button
        Me.CustListView = New System.Windows.Forms.ListView
        Me.CustNameCol = New System.Windows.Forms.ColumnHeader
        Me.CustValueCol = New System.Windows.Forms.ColumnHeader
        Me.CustTypeCol = New System.Windows.Forms.ColumnHeader
        Me.lbCustIntro = New System.Windows.Forms.Label
        Me.PropTabs.SuspendLayout()
        Me.SummaryTab.SuspendLayout()
        Me.StatTab.SuspendLayout()
        Me.CustomTab.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdOpen
        '
        Me.cmdOpen.Location = New System.Drawing.Point(368, 16)
        Me.cmdOpen.Name = "cmdOpen"
        Me.cmdOpen.Size = New System.Drawing.Size(80, 24)
        Me.cmdOpen.TabIndex = 0
        Me.cmdOpen.Text = "Open..."
        '
        'lbFileName
        '
        Me.lbFileName.Location = New System.Drawing.Point(64, 16)
        Me.lbFileName.Name = "lbFileName"
        Me.lbFileName.Size = New System.Drawing.Size(296, 24)
        Me.lbFileName.TabIndex = 2
        Me.lbFileName.Text = "[Click Open button to read properties from file...]"
        '
        'picIcon
        '
        Me.picIcon.Location = New System.Drawing.Point(16, 8)
        Me.picIcon.Name = "picIcon"
        Me.picIcon.Size = New System.Drawing.Size(40, 40)
        Me.picIcon.TabIndex = 3
        Me.picIcon.TabStop = False
        '
        'PropTabs
        '
        Me.PropTabs.Controls.Add(Me.SummaryTab)
        Me.PropTabs.Controls.Add(Me.StatTab)
        Me.PropTabs.Controls.Add(Me.CustomTab)
        Me.PropTabs.Enabled = False
        Me.PropTabs.ItemSize = New System.Drawing.Size(120, 18)
        Me.PropTabs.Location = New System.Drawing.Point(8, 56)
        Me.PropTabs.Name = "PropTabs"
        Me.PropTabs.SelectedIndex = 0
        Me.PropTabs.Size = New System.Drawing.Size(448, 352)
        Me.PropTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
        Me.PropTabs.TabIndex = 1
        '
        'SummaryTab
        '
        Me.SummaryTab.Controls.Add(Me.lbFilePath)
        Me.SummaryTab.Controls.Add(Me.lbSumPath)
        Me.SummaryTab.Controls.Add(Me.lbSumIntro)
        Me.SummaryTab.Controls.Add(Me.txtComments)
        Me.SummaryTab.Controls.Add(Me.lbSumComments)
        Me.SummaryTab.Controls.Add(Me.txtCompany)
        Me.SummaryTab.Controls.Add(Me.lbSumCompany)
        Me.SummaryTab.Controls.Add(Me.txtSubject)
        Me.SummaryTab.Controls.Add(Me.lbSumSubject)
        Me.SummaryTab.Controls.Add(Me.txtAuthor)
        Me.SummaryTab.Controls.Add(Me.lbSumAuthor)
        Me.SummaryTab.Controls.Add(Me.txtTitle)
        Me.SummaryTab.Controls.Add(Me.lbSumTitle)
        Me.SummaryTab.Location = New System.Drawing.Point(4, 22)
        Me.SummaryTab.Name = "SummaryTab"
        Me.SummaryTab.Size = New System.Drawing.Size(440, 326)
        Me.SummaryTab.TabIndex = 0
        Me.SummaryTab.Text = "Summary"
        '
        'lbFilePath
        '
        Me.lbFilePath.Location = New System.Drawing.Point(96, 288)
        Me.lbFilePath.Name = "lbFilePath"
        Me.lbFilePath.Size = New System.Drawing.Size(312, 32)
        Me.lbFilePath.TabIndex = 0
        '
        'lbSumPath
        '
        Me.lbSumPath.Location = New System.Drawing.Point(16, 288)
        Me.lbSumPath.Name = "lbSumPath"
        Me.lbSumPath.Size = New System.Drawing.Size(48, 16)
        Me.lbSumPath.TabIndex = 1
        Me.lbSumPath.Text = "Path:"
        '
        'lbSumIntro
        '
        Me.lbSumIntro.Location = New System.Drawing.Point(16, 8)
        Me.lbSumIntro.Name = "lbSumIntro"
        Me.lbSumIntro.Size = New System.Drawing.Size(344, 16)
        Me.lbSumIntro.TabIndex = 2
        Me.lbSumIntro.Text = "Summary Document Properties:"
        '
        'txtComments
        '
        Me.txtComments.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtComments.Location = New System.Drawing.Point(88, 168)
        Me.txtComments.Multiline = True
        Me.txtComments.Name = "txtComments"
        Me.txtComments.Size = New System.Drawing.Size(328, 104)
        Me.txtComments.TabIndex = 9
        Me.txtComments.Text = ""
        '
        'lbSumComments
        '
        Me.lbSumComments.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSumComments.Location = New System.Drawing.Point(16, 168)
        Me.lbSumComments.Name = "lbSumComments"
        Me.lbSumComments.Size = New System.Drawing.Size(64, 16)
        Me.lbSumComments.TabIndex = 10
        Me.lbSumComments.Text = "Comments:"
        '
        'txtCompany
        '
        Me.txtCompany.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCompany.Location = New System.Drawing.Point(88, 136)
        Me.txtCompany.Name = "txtCompany"
        Me.txtCompany.Size = New System.Drawing.Size(328, 20)
        Me.txtCompany.TabIndex = 7
        Me.txtCompany.Text = ""
        '
        'lbSumCompany
        '
        Me.lbSumCompany.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSumCompany.Location = New System.Drawing.Point(16, 136)
        Me.lbSumCompany.Name = "lbSumCompany"
        Me.lbSumCompany.Size = New System.Drawing.Size(64, 16)
        Me.lbSumCompany.TabIndex = 11
        Me.lbSumCompany.Text = "Company:"
        '
        'txtSubject
        '
        Me.txtSubject.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSubject.Location = New System.Drawing.Point(88, 104)
        Me.txtSubject.Name = "txtSubject"
        Me.txtSubject.Size = New System.Drawing.Size(328, 20)
        Me.txtSubject.TabIndex = 3
        Me.txtSubject.Text = ""
        '
        'lbSumSubject
        '
        Me.lbSumSubject.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSumSubject.Location = New System.Drawing.Point(16, 104)
        Me.lbSumSubject.Name = "lbSumSubject"
        Me.lbSumSubject.Size = New System.Drawing.Size(64, 16)
        Me.lbSumSubject.TabIndex = 12
        Me.lbSumSubject.Text = "Subject:"
        '
        'txtAuthor
        '
        Me.txtAuthor.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtAuthor.Location = New System.Drawing.Point(88, 72)
        Me.txtAuthor.Name = "txtAuthor"
        Me.txtAuthor.Size = New System.Drawing.Size(328, 20)
        Me.txtAuthor.TabIndex = 2
        Me.txtAuthor.Text = ""
        '
        'lbSumAuthor
        '
        Me.lbSumAuthor.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSumAuthor.Location = New System.Drawing.Point(16, 72)
        Me.lbSumAuthor.Name = "lbSumAuthor"
        Me.lbSumAuthor.Size = New System.Drawing.Size(64, 16)
        Me.lbSumAuthor.TabIndex = 13
        Me.lbSumAuthor.Text = "Author:"
        '
        'txtTitle
        '
        Me.txtTitle.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtTitle.Location = New System.Drawing.Point(88, 40)
        Me.txtTitle.Name = "txtTitle"
        Me.txtTitle.Size = New System.Drawing.Size(328, 20)
        Me.txtTitle.TabIndex = 1
        Me.txtTitle.Text = ""
        '
        'lbSumTitle
        '
        Me.lbSumTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSumTitle.Location = New System.Drawing.Point(16, 40)
        Me.lbSumTitle.Name = "lbSumTitle"
        Me.lbSumTitle.Size = New System.Drawing.Size(64, 16)
        Me.lbSumTitle.TabIndex = 14
        Me.lbSumTitle.Text = "Title:"
        '
        'StatTab
        '
        Me.StatTab.Controls.Add(Me.lbStatNote)
        Me.StatTab.Controls.Add(Me.lbStatIntro)
        Me.StatTab.Controls.Add(Me.StatListView)
        Me.StatTab.Location = New System.Drawing.Point(4, 22)
        Me.StatTab.Name = "StatTab"
        Me.StatTab.Size = New System.Drawing.Size(440, 326)
        Me.StatTab.TabIndex = 3
        Me.StatTab.Text = "Statistics"
        '
        'lbStatNote
        '
        Me.lbStatNote.Location = New System.Drawing.Point(16, 296)
        Me.lbStatNote.Name = "lbStatNote"
        Me.lbStatNote.Size = New System.Drawing.Size(400, 16)
        Me.lbStatNote.TabIndex = 0
        Me.lbStatNote.Text = "Note: Not all properties are used by all applications. Some items may be blank."
        '
        'lbStatIntro
        '
        Me.lbStatIntro.Location = New System.Drawing.Point(16, 8)
        Me.lbStatIntro.Name = "lbStatIntro"
        Me.lbStatIntro.Size = New System.Drawing.Size(344, 16)
        Me.lbStatIntro.TabIndex = 1
        Me.lbStatIntro.Text = "Document Statistics:"
        '
        'StatListView
        '
        Me.StatListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.StatNameCol, Me.StatValCol})
        Me.StatListView.GridLines = True
        Me.StatListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.StatListView.Location = New System.Drawing.Point(16, 32)
        Me.StatListView.Name = "StatListView"
        Me.StatListView.Size = New System.Drawing.Size(408, 256)
        Me.StatListView.TabIndex = 0
        Me.StatListView.View = System.Windows.Forms.View.Details
        '
        'StatNameCol
        '
        Me.StatNameCol.Text = "Name"
        Me.StatNameCol.Width = 140
        '
        'StatValCol
        '
        Me.StatValCol.Text = "Value"
        Me.StatValCol.Width = 240
        '
        'CustomTab
        '
        Me.CustomTab.Controls.Add(Me.cboxCustType)
        Me.CustomTab.Controls.Add(Me.lbCustType)
        Me.CustomTab.Controls.Add(Me.lbCustValue)
        Me.CustomTab.Controls.Add(Me.lbCustName)
        Me.CustomTab.Controls.Add(Me.lbCustNote)
        Me.CustomTab.Controls.Add(Me.txtCustValue)
        Me.CustomTab.Controls.Add(Me.txtCustName)
        Me.CustomTab.Controls.Add(Me.cmdRemove)
        Me.CustomTab.Controls.Add(Me.cmdAdd)
        Me.CustomTab.Controls.Add(Me.CustListView)
        Me.CustomTab.Controls.Add(Me.lbCustIntro)
        Me.CustomTab.Location = New System.Drawing.Point(4, 22)
        Me.CustomTab.Name = "CustomTab"
        Me.CustomTab.Size = New System.Drawing.Size(440, 326)
        Me.CustomTab.TabIndex = 1
        Me.CustomTab.Text = "Custom"
        '
        'cboxCustType
        '
        Me.cboxCustType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboxCustType.Items.AddRange(New Object() {"String", "Long", "Double", "Boolean", "Date"})
        Me.cboxCustType.Location = New System.Drawing.Point(264, 248)
        Me.cboxCustType.Name = "cboxCustType"
        Me.cboxCustType.Size = New System.Drawing.Size(144, 21)
        Me.cboxCustType.TabIndex = 3
        '
        'lbCustType
        '
        Me.lbCustType.Location = New System.Drawing.Point(224, 248)
        Me.lbCustType.Name = "lbCustType"
        Me.lbCustType.Size = New System.Drawing.Size(40, 16)
        Me.lbCustType.TabIndex = 4
        Me.lbCustType.Text = "Type:"
        '
        'lbCustValue
        '
        Me.lbCustValue.Location = New System.Drawing.Point(16, 280)
        Me.lbCustValue.Name = "lbCustValue"
        Me.lbCustValue.Size = New System.Drawing.Size(40, 16)
        Me.lbCustValue.TabIndex = 5
        Me.lbCustValue.Text = "Value:"
        '
        'lbCustName
        '
        Me.lbCustName.Location = New System.Drawing.Point(16, 248)
        Me.lbCustName.Name = "lbCustName"
        Me.lbCustName.Size = New System.Drawing.Size(40, 16)
        Me.lbCustName.TabIndex = 6
        Me.lbCustName.Text = "Name:"
        '
        'lbCustNote
        '
        Me.lbCustNote.Location = New System.Drawing.Point(16, 208)
        Me.lbCustNote.Name = "lbCustNote"
        Me.lbCustNote.Size = New System.Drawing.Size(216, 32)
        Me.lbCustNote.TabIndex = 7
        Me.lbCustNote.Text = "To add a new item, fill in the information below and click the Add button."
        '
        'txtCustValue
        '
        Me.txtCustValue.Location = New System.Drawing.Point(64, 280)
        Me.txtCustValue.Name = "txtCustValue"
        Me.txtCustValue.Size = New System.Drawing.Size(336, 20)
        Me.txtCustValue.TabIndex = 4
        Me.txtCustValue.Text = ""
        '
        'txtCustName
        '
        Me.txtCustName.Location = New System.Drawing.Point(64, 248)
        Me.txtCustName.Name = "txtCustName"
        Me.txtCustName.Size = New System.Drawing.Size(144, 20)
        Me.txtCustName.TabIndex = 2
        Me.txtCustName.Text = ""
        '
        'cmdRemove
        '
        Me.cmdRemove.Enabled = False
        Me.cmdRemove.Location = New System.Drawing.Point(328, 200)
        Me.cmdRemove.Name = "cmdRemove"
        Me.cmdRemove.Size = New System.Drawing.Size(80, 24)
        Me.cmdRemove.TabIndex = 6
        Me.cmdRemove.Text = "Remove"
        '
        'cmdAdd
        '
        Me.cmdAdd.Location = New System.Drawing.Point(240, 200)
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Size = New System.Drawing.Size(80, 24)
        Me.cmdAdd.TabIndex = 5
        Me.cmdAdd.Text = "Add"
        '
        'CustListView
        '
        Me.CustListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.CustNameCol, Me.CustValueCol, Me.CustTypeCol})
        Me.CustListView.FullRowSelect = True
        Me.CustListView.GridLines = True
        Me.CustListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.CustListView.HideSelection = False
        Me.CustListView.Location = New System.Drawing.Point(16, 32)
        Me.CustListView.MultiSelect = False
        Me.CustListView.Name = "CustListView"
        Me.CustListView.Size = New System.Drawing.Size(408, 160)
        Me.CustListView.TabIndex = 1
        Me.CustListView.View = System.Windows.Forms.View.Details
        '
        'CustNameCol
        '
        Me.CustNameCol.Text = "Name"
        Me.CustNameCol.Width = 120
        '
        'CustValueCol
        '
        Me.CustValueCol.Text = "Value"
        Me.CustValueCol.Width = 180
        '
        'CustTypeCol
        '
        Me.CustTypeCol.Text = "Type"
        Me.CustTypeCol.Width = 80
        '
        'lbCustIntro
        '
        Me.lbCustIntro.Location = New System.Drawing.Point(16, 8)
        Me.lbCustIntro.Name = "lbCustIntro"
        Me.lbCustIntro.Size = New System.Drawing.Size(344, 16)
        Me.lbCustIntro.TabIndex = 8
        Me.lbCustIntro.Text = "Custom Document Properties:"
        '
        'FilePropDemo
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(466, 416)
        Me.Controls.Add(Me.cmdOpen)
        Me.Controls.Add(Me.PropTabs)
        Me.Controls.Add(Me.lbFileName)
        Me.Controls.Add(Me.picIcon)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "FilePropDemo"
        Me.Text = "Document Properties Sample"
        Me.PropTabs.ResumeLayout(False)
        Me.SummaryTab.ResumeLayout(False)
        Me.StatTab.ResumeLayout(False)
        Me.CustomTab.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
#End Region

    ' OpenDocumentProperties:
    '  Creates a new DSOFile.OleDocumentPropertiesClass and opens a
    '  file choosen by the user. The properties are filled into the
    '  dialog and command tabs are enabled...
    Public Function OpenDocumentProperties() As Boolean
        Dim oSummProps As DSOFile.SummaryProperties
        Dim oCustProp As DSOFile.CustomProperty
        Dim oDialog As OpenFileDialog
        Dim sFile, sTmp As String
        Dim fOpenReadOnly As Boolean

        ' Ask the user for an Office OLE Structure Storage file to read
        ' the document properties from. You can also select any common file
        ' on a Win2K/WinXP NTFS drive, our a non-Office OLE file...
        oDialog = New OpenFileDialog
        oDialog.Filter = c_strFilter
        oDialog.FilterIndex = 1
        oDialog.ShowReadOnly = True
        oDialog.CheckFileExists = True
        If oDialog.ShowDialog() = DialogResult.Cancel Then
            ' Nothing to do is user cancels...
            OpenDocumentProperties = False
            Exit Function
        End If
        sFile = oDialog.FileName
        fOpenReadOnly = oDialog.ReadOnlyChecked

        ' Create the OleDocumentProperties object and open the file. The
        ' dsoOptionOpenReadOnlyIfNoWriteAccess allows us to open the file
        ' read/write if we have access, but go ahead and open read-only if
        ' we don't. Since viewing properties is main purpose of the sample
        ' it is OK for us to fail write access lock on this open...
        m_oDocument = New DSOFile.OleDocumentPropertiesClass
        m_oDocument.Open(sFile, fOpenReadOnly, DSOFile.dsoFileOpenOptions.dsoOptionOpenReadOnlyIfNoWriteAccess)

        ' Here we can tell if file was open read-only...
        m_fOpenedReadOnly = m_oDocument.IsReadOnly

        ' Display the basic file info...
        lbFileName.Text = m_oDocument.Name
        lbFilePath.Text = m_oDocument.Path
        picIcon.Image = GetIconImageFromDisp(m_oDocument.Icon)

        ' Get the SummaryProperties (these are built-in set)...
        oSummProps = m_oDocument.SummaryProperties

        ' We'll load a few of these properties into text boxes which we
        ' can change in this sample. Other properties can be changed
        ' as well, but not by this sample...
        txtTitle.Text = oSummProps.Title
        txtAuthor.Text = oSummProps.Author
        txtSubject.Text = oSummProps.Subject
        txtCompany.Text = oSummProps.Company
        txtComments.Text = oSummProps.Comments

        ' Fill in the Summary/Statistics information in the properties list.
        ' These properties are the standard Summay and Document OLE Properties...
        Dim lvItem As ListViewItem
        lvItem = GetLvItemForProperty("Application", oSummProps.ApplicationName)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Version", oSummProps.Version)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Category", oSummProps.Category)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Keywords", oSummProps.Keywords)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Manager", oSummProps.Manager)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Last Author", oSummProps.LastSavedBy)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Word Count", oSummProps.WordCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Page Count", oSummProps.PageCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Paragraph Count", oSummProps.ParagraphCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Line Count", oSummProps.LineCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Character Count", oSummProps.CharacterCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Character Count (w/spaces)", oSummProps.CharacterCountWithSpaces)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Byte Count", oSummProps.ByteCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Presentation Format", oSummProps.PresentationFormat)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Slide Count", oSummProps.SlideCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Note Count", oSummProps.NoteCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Hidden Slides", oSummProps.HiddenSlideCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Multimedia Clips", oSummProps.MultimediaClipCount)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Date Created", oSummProps.DateCreated)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Date Last Printed", oSummProps.DateLastPrinted)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Date Last Saved", oSummProps.DateLastSaved)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Editing Time (mins)", oSummProps.TotalEditTime)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Template", oSummProps.Template)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("Revision", oSummProps.RevisionNumber)
        StatListView.Items.Add(lvItem)
        lvItem = GetLvItemForProperty("IsShared", oSummProps.SharedDocument)
        StatListView.Items.Add(lvItem)

        ' Add a few other items that pertain to OLE files only...
        If m_oDocument.IsOleFile Then
            lvItem = GetLvItemForProperty("CLSID", m_oDocument.CLSID)
            StatListView.Items.Add(lvItem)
            lvItem = GetLvItemForProperty("ProgID", m_oDocument.ProgID)
            StatListView.Items.Add(lvItem)
            lvItem = GetLvItemForProperty("OleFormat", m_oDocument.OleDocumentFormat)
            StatListView.Items.Add(lvItem)
            lvItem = GetLvItemForProperty("OleType", m_oDocument.OleDocumentType)
            StatListView.Items.Add(lvItem)
        End If

        ' Now load the custom properties...
        For Each oCustProp In m_oDocument.CustomProperties
            If oCustProp.Type <> DSOFile.dsoFilePropertyType.dsoPropertyTypeUnknown Then
                lvItem = GetLvItemForProperty(oCustProp.Name, CStr(oCustProp.Value), oCustProp.Type)
                CustListView.Items.Add(lvItem)
            End If
        Next oCustProp

        ' Enable/Disable text items if file is open read only...
        EnableItems(Not m_fOpenedReadOnly)

        ' The operation was successful.
        OpenDocumentProperties = True
        PropTabs.Enabled = True
    End Function

    ' CloseCurrentDocument:
    '  Closed the open document and clears the dialog. Can prompt the user
    '  to save the changes made if open read-write mode.
    Private Sub CloseCurrentDocument(ByVal bPromptToSaveIfDirty As Boolean)
        If Not (m_oDocument Is Nothing) Then
            ' If changes where made, ask user if they want to save...
            If bPromptToSaveIfDirty And m_oDocument.IsDirty And Not m_fOpenedReadOnly Then
                If MsgBox("Would you like to save the changes made to this file?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    m_oDocument.Save()
                End If
            End If
            ' Close before exit...
            m_oDocument.Close()
            m_oDocument = Nothing
        End If
        ' Reset all the control values to default...
        ClearControls()
        PropTabs.Enabled = False
    End Sub

    ' EnableItems:
    '   Helper function to enable/disable controls with respect to
    '   read-only mode for the current file...
    Private Sub EnableItems(ByVal bEnable As Boolean)
        txtTitle.Enabled = bEnable : txtAuthor.Enabled = bEnable
        txtSubject.Enabled = bEnable : txtCompany.Enabled = bEnable
        txtComments.Enabled = bEnable : txtCustName.Enabled = bEnable
        txtCustValue.Enabled = bEnable : cboxCustType.Enabled = bEnable
        cmdAdd.Enabled = bEnable
    End Sub

    ' ClearControls:
    '  Helper function to restore dialog controls to "blank" slate...
    Private Sub ClearControls()
        lbFileName.Text = "[Click Open button to read properties from file...]"
        lbFilePath.Text = "" : txtTitle.Text = ""
        txtAuthor.Text = "" : txtSubject.Text = ""
        txtCompany.Text = "" : txtComments.Text = ""
        txtCustName.Text = "" : txtCustValue.Text = ""
        cboxCustType.SelectedIndex = 0
        picIcon.Image = Nothing
        StatListView.Items.Clear()
        CustListView.Items.Clear()
        cmdRemove.Enabled = False
    End Sub

    ' GetLvItemForProperty:
    '  Helper function to take name and value and return ListViewItem 
    '  which can be added to the listview for the propset being displayed.
    Private Function GetLvItemForProperty(ByVal sName As String, ByVal sValue As String, _
        Optional ByVal iType As DSOFile.dsoFilePropertyType = DSOFile.dsoFilePropertyType.dsoPropertyTypeUnknown)
        Dim lvItem As New ListViewItem
        Dim sTypeName As String
        lvItem.Text = sName
        lvItem.SubItems.Add(sValue)
        Select Case iType
            Case DSOFile.dsoFilePropertyType.dsoPropertyTypeString
                sTypeName = "String"
            Case DSOFile.dsoFilePropertyType.dsoPropertyTypeDouble
                sTypeName = "Double"
            Case DSOFile.dsoFilePropertyType.dsoPropertyTypeLong
                sTypeName = "Long"
            Case DSOFile.dsoFilePropertyType.dsoPropertyTypeBool
                sTypeName = "Boolean"
            Case DSOFile.dsoFilePropertyType.dsoPropertyTypeDate
                sTypeName = "Date"
            Case Else
                sTypeName = "Unknown"
        End Select
        lvItem.SubItems.Add(sTypeName)
        GetLvItemForProperty = lvItem
    End Function

    ' GetIconImageFromDisp:
    '  Helper function to take PictureDisp and create GDI+ Bitmap Image for 
    '  the file icon to display in WinForm PictureBox...
    Private Function GetIconImageFromDisp(ByVal oDispPicture As Object) As System.Drawing.Image
        Dim iType As Integer
        Dim iHandle As Integer
        Dim args() As Object
        Try
            ' Confirm that object contains an ICON picture...
            iType = CLng(oDispPicture.GetType.InvokeMember("Type", Reflection.BindingFlags.GetProperty, Nothing, oDispPicture, args))
            If iType = 3 Then ' If So, ask for the handle...
                iHandle = oDispPicture.GetType.InvokeMember("Handle", Reflection.BindingFlags.GetProperty, Nothing, oDispPicture, args)
                ' Create the Drawing.Bitmap object from the ICON handle...
                GetIconImageFromDisp = System.Drawing.Bitmap.FromHicon(New System.IntPtr(iHandle))
            End If
        Catch ex As Exception
            ' Return Nothing if exception thrown..
        End Try
    End Function

    ' cmdOpen_Click: Handler for Open Button
    Private Sub cmdOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOpen.Click
        CloseCurrentDocument(True)
        If Not OpenDocumentProperties() Then
            CloseCurrentDocument(False)
        End If
    End Sub

    ' cmdAdd_Click: Handler for Custom Property Add Button
    Private Sub cmdAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAdd.Click
        Dim oCustProp As DSOFile.CustomProperty
        Dim sName As String, sValueText As String
        Dim vValue As Object

        sName = txtCustName.Text
        sValueText = txtCustValue.Text

        ' We can't add a custom property unless we have a valid name
        ' and value, so we do a quick check now to avoid error later on.
        If ((sName = "") Or (sValueText = "")) Then
            Beep() : txtCustName.Select()
            Exit Sub
        End If

        Try
            ' Convert the Text string to an object of the type
            ' specified in the drop down list.
            Select Case cboxCustType.SelectedIndex + 1
                Case 2
                    vValue = CInt(sValueText)
                Case 3
                    vValue = CDbl(sValueText)
                Case 4
                    vValue = CBool(sValueText)
                Case 5
                    vValue = CDate(sValueText)
                Case Else
                    vValue = sValueText
            End Select

            ' Add the property to the custom properties list...
            oCustProp = m_oDocument.CustomProperties.Add(sName, vValue)

            ' If that succeeded, add the item to the list view...
            CustListView.Items.Add(GetLvItemForProperty(sName, sValueText, oCustProp.Type))
            txtCustName.Text = "" : txtCustValue.Text = ""

        Catch ex As Exception
            MsgBox("The item could not be added!" & vbCrLf & "Error: " & ex.Message, MsgBoxStyle.Critical)
        End Try

    End Sub

    ' cmdRemove_Click: Handler for Custom Property Remove Button
    Private Sub cmdRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRemove.Click
        Dim oRmProp As DSOFile.CustomProperty
        Dim iRemoveCnt As Integer, i As Integer
        Dim sName As String

        ' Get the count of selected items (normally this is 1)...
        iRemoveCnt = CustListView.SelectedItems.Count
        If iRemoveCnt < 1 Then ' This should not happen, but just in case...
            MsgBox("There is no selected item to remove!", MsgBoxStyle.Critical)
            Exit Sub
        End If

        ' For each fo the selected items, remove them from the document.
        ' We loop backwards to not change the list as we also want to 
        ' remove the listview item when the property is removed...
        For i = iRemoveCnt - 1 To 0 Step -1
            sName = CustListView.SelectedItems(i).Text
            Try
                oRmProp = m_oDocument.CustomProperties.Item(sName)
                oRmProp.Remove()
                oRmProp = Nothing
                CustListView.SelectedItems(i).Remove()
            Catch ex As Exception
                MsgBox("Unable to remove '" & sName & "' from document!" & _
                    vbCrLf & "Error: " & ex.Message, MsgBoxStyle.Critical)
            End Try
        Next

        cmdRemove.Enabled = False
    End Sub

    ' FilePropDemo_Closing: Cleanup before exit and ask user to save changes if needed...
    Private Sub FilePropDemo_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        CloseCurrentDocument(True)
    End Sub

    ' CustListView_SelectedIndexChanged: Enable Remove button if item selected.
    Private Sub CustListView_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CustListView.SelectedIndexChanged
        If (Not m_fOpenedReadOnly) Then
            cmdRemove.Enabled = CustListView.SelectedItems.Count > 0
        End If
    End Sub

#Region "Field Validation (used to update changable text properties)"
    ' We use WinForm TextBox Validation events to write the changed values of the
    ' text boxes back to the OleDocumentPropertiesClass. We only do this if the 
    ' value changes from the current setting. Once we update the OleDocumentPropertiesClass
    ' the document will be marked dirty. The changes are not actually made to the file
    ' until the document is Saved, which we do on cleanup/exit if user says Yes to prompt.
    Private Sub txtTitle_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTitle.Validated
        If Not m_oDocument Is Nothing Then
            Dim oSummProps As DSOFile.SummaryProperties = m_oDocument.SummaryProperties
            If (txtTitle.Text <> oSummProps.Title) Then
                oSummProps.Title = txtTitle.Text
            End If
        End If
    End Sub

    Private Sub txtAuthor_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAuthor.Validated
        If Not m_oDocument Is Nothing Then
            Dim oSummProps As DSOFile.SummaryProperties = m_oDocument.SummaryProperties
            If (txtAuthor.Text <> oSummProps.Author) Then
                oSummProps.Author = txtAuthor.Text
            End If
        End If
    End Sub

    Private Sub txtSubject_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSubject.Validated
        If Not m_oDocument Is Nothing Then
            Dim oSummProps As DSOFile.SummaryProperties = m_oDocument.SummaryProperties
            If (txtSubject.Text <> oSummProps.Subject) Then
                oSummProps.Subject = txtSubject.Text
            End If
        End If
    End Sub

    Private Sub txtCompany_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtCompany.Validated
        If Not m_oDocument Is Nothing Then
            Dim oSummProps As DSOFile.SummaryProperties = m_oDocument.SummaryProperties
            If (txtCompany.Text <> oSummProps.Company) Then
                oSummProps.Company = txtCompany.Text
            End If
        End If
    End Sub

    Private Sub txtComments_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtComments.Validated
        If Not m_oDocument Is Nothing Then
            Dim oSummProps As DSOFile.SummaryProperties = m_oDocument.SummaryProperties
            If (txtComments.Text <> oSummProps.Comments) Then
                oSummProps.Comments = txtComments.Text
            End If
        End If
    End Sub
#End Region


End Class
