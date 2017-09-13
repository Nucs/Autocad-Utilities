VERSION 5.00
Object = "{F9043C88-F6F2-101A-A3C9-08002B2F49FB}#1.2#0"; "comdlg32.ocx"
Begin VB.Form FilePropDemo 
   BorderStyle     =   3  'Fixed Dialog
   Caption         =   "Document Properties"
   ClientHeight    =   7365
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   5655
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   7365
   ScaleWidth      =   5655
   StartUpPosition =   1  'CenterOwner
   Begin VB.PictureBox picPreview 
      BackColor       =   &H00FFFFFF&
      Height          =   3675
      Left            =   -20000
      ScaleHeight     =   3615
      ScaleWidth      =   4395
      TabIndex        =   19
      Top             =   600
      Width           =   4455
   End
   Begin VB.CheckBox chkShowPreview 
      Caption         =   "Preview Document"
      Enabled         =   0   'False
      Height          =   225
      Left            =   480
      TabIndex        =   18
      Top             =   4320
      Width           =   1680
   End
   Begin VB.CommandButton cmdCustAdd 
      Caption         =   "Add"
      Height          =   345
      Left            =   3030
      TabIndex        =   17
      Top             =   5475
      Width           =   960
   End
   Begin VB.CommandButton cmdCustRemove 
      Caption         =   "Remove"
      Enabled         =   0   'False
      Height          =   345
      Left            =   4170
      TabIndex        =   16
      Top             =   5475
      Width           =   990
   End
   Begin VB.ListBox lstSummProps 
      Height          =   2010
      Left            =   360
      TabIndex        =   15
      Top             =   2160
      Width           =   4935
   End
   Begin VB.TextBox txtAuthor 
      Height          =   330
      Left            =   1140
      TabIndex        =   14
      Top             =   1155
      Width           =   4215
   End
   Begin VB.TextBox txtComments 
      Height          =   330
      Left            =   1140
      TabIndex        =   13
      Top             =   1530
      Width           =   4215
   End
   Begin VB.ListBox lstCustProps 
      Height          =   1230
      Left            =   300
      TabIndex        =   12
      Top             =   5895
      Width           =   4995
   End
   Begin MSComDlg.CommonDialog CommonDialog1 
      Left            =   0
      Top             =   0
      _ExtentX        =   847
      _ExtentY        =   847
      _Version        =   393216
   End
   Begin VB.CommandButton cmdOpenFile 
      Caption         =   "New..."
      Height          =   390
      Left            =   4320
      TabIndex        =   0
      Top             =   135
      Width           =   990
   End
   Begin VB.ComboBox lstCustType 
      Height          =   315
      Left            =   3405
      Style           =   2  'Dropdown List
      TabIndex        =   2
      Top             =   5070
      Width           =   1935
   End
   Begin VB.TextBox txtCustValue 
      Height          =   315
      Left            =   870
      TabIndex        =   3
      Top             =   5430
      Width           =   1755
   End
   Begin VB.TextBox txtCustName 
      Height          =   315
      Left            =   870
      TabIndex        =   1
      Top             =   5070
      Width           =   1755
   End
   Begin VB.TextBox txtTitle 
      Height          =   330
      Left            =   1140
      TabIndex        =   20
      Top             =   780
      Width           =   4215
   End
   Begin VB.Label Label11 
      Caption         =   "Custom Properties:"
      Height          =   255
      Left            =   240
      TabIndex        =   11
      Top             =   4770
      Width           =   1425
   End
   Begin VB.Line Line8 
      BorderColor     =   &H80000010&
      X1              =   255
      X2              =   5330
      Y1              =   1995
      Y2              =   1995
   End
   Begin VB.Line Line7 
      BorderColor     =   &H80000014&
      X1              =   255
      X2              =   5330
      Y1              =   2010
      Y2              =   2010
   End
   Begin VB.Label Label7 
      Caption         =   "Value:"
      Height          =   210
      Left            =   240
      TabIndex        =   10
      Top             =   5490
      Width           =   525
   End
   Begin VB.Label Label6 
      Caption         =   "Type:"
      Height          =   225
      Left            =   2850
      TabIndex        =   9
      Top             =   5130
      Width           =   450
   End
   Begin VB.Label Label5 
      Caption         =   "Name:"
      Height          =   195
      Left            =   240
      TabIndex        =   8
      Top             =   5100
      Width           =   510
   End
   Begin VB.Label Label4 
      Caption         =   "Title:"
      Height          =   195
      Left            =   255
      TabIndex        =   7
      Top             =   855
      Width           =   480
   End
   Begin VB.Line Line5 
      BorderColor     =   &H80000014&
      X1              =   250
      X2              =   5325
      Y1              =   4650
      Y2              =   4650
   End
   Begin VB.Line Line6 
      BorderColor     =   &H80000010&
      X1              =   250
      X2              =   5325
      Y1              =   4635
      Y2              =   4635
   End
   Begin VB.Image imgIcon 
      Height          =   585
      Left            =   240
      Top             =   90
      Width           =   585
   End
   Begin VB.Label lbName 
      Height          =   555
      Left            =   1140
      TabIndex        =   6
      Top             =   120
      Width           =   3150
   End
   Begin VB.Line Line1 
      BorderColor     =   &H80000010&
      X1              =   250
      X2              =   5325
      Y1              =   690
      Y2              =   690
   End
   Begin VB.Line Line2 
      BorderColor     =   &H80000014&
      X1              =   250
      X2              =   5325
      Y1              =   705
      Y2              =   705
   End
   Begin VB.Label Label3 
      Caption         =   "Comments:"
      Height          =   195
      Left            =   225
      TabIndex        =   5
      Top             =   1620
      Width           =   780
   End
   Begin VB.Label Label8 
      Caption         =   "Author:"
      Height          =   195
      Left            =   240
      TabIndex        =   4
      Top             =   1245
      Width           =   570
   End
End
Attribute VB_Name = "FilePropDemo"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'*********************************************************************
' FileProp.frm
'
' VB6 Sample code for reading file properties using DSOFILE 2.1
'
'  Copyright (c)1999-2000 Microsoft Corporation, All Rights Reserved
'  Microsoft Product Support Services, Developer Support
'
'  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
'  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
'  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR
'  A PARTICULAR PURPOSE.
'
'*********************************************************************
Option Explicit

'*********************************************************************
' FileProp Form Member Variables
'*********************************************************************
' Uses reference to "DSO OLE Document Properties Reader 2.1"
Private m_oDocumentProps As DSOFile.OleDocumentProperties

Const c_strGlobalFileFilter As String = _
    "All Office Files|*.doc;*.docx;*.docm;*.dot;*.dotx;*.xls;*.xlsx;*.xlsm;*.xlsb;*.xla;*xlam;*.ppt;*.pptx;*.pptm;*.vsd;*.mpp|All Files|*.*"

'*********************************************************************
' Form_Load  -- Creates new instance of the component. Failure here
'   will result in termination of the application. You need to register
'   the DLL before building and running this sample!!
'*********************************************************************
Private Sub Form_Load()

 ' Create an instance of the DSOFile Object...
   Set m_oDocumentProps = New DSOFile.OleDocumentProperties
      
 ' Fill in the default items for the DataType list...
   lstCustType.AddItem "String"
   lstCustType.AddItem "Long"
   lstCustType.AddItem "Double"
   lstCustType.AddItem "Boolean"
   lstCustType.AddItem "Date"

 ' Show the form...
   Me.Show
      
 ' Pick a file and open the properties for it,
   If Not OpenDocumentProperties Then
      End ' If user cancels, we exit...
   End If
   
End Sub

'*********************************************************************
' Form_Unload  -- Closes the dialog and prompts user to save (if dirty).
'*********************************************************************
Private Sub Form_Unload(Cancel As Integer)
   CloseCurrentDocument True
   Set m_oDocumentProps = Nothing
End Sub

'*********************************************************************
' cmdOpenFile_Click  -- Closes the current file and opens new one.
'*********************************************************************
Private Sub cmdOpenFile_Click()
   CloseCurrentDocument True
   OpenDocumentProperties
End Sub

'*********************************************************************
' cmdOpenFile_Click  -- Shows/Hides Preview image (if document has one).
'*********************************************************************
Private Sub chkShowPreview_Click()
 ' The preview is loaded in a picture box. When this item is
 ' checked, move the picture box on screen. Otherwise, move off..
   If chkShowPreview.Value Then
      lstSummProps.Left = -20000
      picPreview.Left = 960
   Else
      picPreview.Left = -20000
      lstSummProps.Left = 360
   End If
End Sub

'*********************************************************************
' OpenDocumentProperties -- Fills the dialog with properties
'     from a user supplied Office OLE Structured Storage document.
'*********************************************************************
Public Function OpenDocumentProperties() As Boolean
 ' Lists both Summary and Custom Properties...
   Dim oSummProps As DSOFile.SummaryProperties
   Dim oCustProp As DSOFile.CustomProperty
   Dim sFile As String, sTmp As String
   Dim fOpenReadOnly As Boolean
   Dim oPicDisp As StdPicture
   
 ' Simple Error handler for this sample...
   On Error GoTo Err_Trap
   
 ' Ask the user for an Office OLE Structure Storage file to read
 ' the document properties from. You can also select any common file
 ' on a Win2K/WinXP NTFS drive, our a non-Office OLE file...
   With CommonDialog1
      .Flags = cdlOFNFileMustExist
      .Filter = c_strGlobalFileFilter
      .FileName = ""
      .ShowOpen
      sFile = .FileName
      fOpenReadOnly = CBool(.Flags And cdlOFNReadOnly)
   End With
   
 ' If the user cancels the dialog, exit out.
   If Len(sFile) = 0 Then Exit Function
   
 ' Clear out existing settings...
   lbName.Caption = ""
   lstSummProps.Clear
   txtCustName.Text = ""
   txtCustValue.Text = ""
   lstCustType.ListIndex = 0
   lstCustProps.Clear
   Set imgIcon.Picture = Nothing
   
 ' Here is where we load the document properties for the file.
 ' Depending on the read-only option set in the Open dialog, we
 ' can open for editing or just read-only. If you pass False to
 ' read-only flag and the file cannot be edited (because of access)
 ' an error will occur. For simple case, we can pass the optional flag
 ' ask the component to switch to read-only if file is protected.
   m_oDocumentProps.Open sFile, fOpenReadOnly, dsoOptionOpenReadOnlyIfNoWriteAccess
   
 ' We just display the file name for the caption...
   lbName.Caption = m_oDocumentProps.Name
   
 ' Get the associated icon picture for the file type...
   Set imgIcon.Picture = m_oDocumentProps.Icon
   
 ' Get the SummaryProperties (these are built-in set)...
   Set oSummProps = m_oDocumentProps.SummaryProperties
   
 ' We'll load a few of these properties into text boxes which we
 ' can change in this sample. Other properties can be changed
 ' as well, but not by this sample...
   txtTitle.Text = oSummProps.Title
   txtAuthor.Text = oSummProps.Author
   txtComments.Text = oSummProps.Comments
   
 ' Fill in the Summary/Statistics information in the properties list.
 ' These properties are the standard Summay and Document OLE Properties...
   lstSummProps.AddItem "Application:  " & oSummProps.ApplicationName
   lstSummProps.AddItem "Version:      " & oSummProps.Version
   lstSummProps.AddItem "Subject:      " & oSummProps.Subject
   lstSummProps.AddItem "Category:     " & oSummProps.Category
   lstSummProps.AddItem "Company:      " & oSummProps.Company
   lstSummProps.AddItem "Keywords:     " & oSummProps.Keywords
   lstSummProps.AddItem "Manager:      " & oSummProps.Manager
   lstSummProps.AddItem "LastSaved by: " & oSummProps.LastSavedBy
   lstSummProps.AddItem "WordCount:    " & oSummProps.WordCount
   lstSummProps.AddItem "PageCount:    " & oSummProps.PageCount
   lstSummProps.AddItem "ParagraphCount: " & oSummProps.ParagraphCount
   lstSummProps.AddItem "LineCount:    " & oSummProps.LineCount
   lstSummProps.AddItem "CharacterCount: " & oSummProps.CharacterCount
   lstSummProps.AddItem "CharacterCount (w/spaces): " & oSummProps.CharacterCountWithSpaces
   lstSummProps.AddItem "ByteCount:    " & oSummProps.ByteCount
   lstSummProps.AddItem "PresFormat:   " & oSummProps.PresentationFormat
   lstSummProps.AddItem "SlideCount:   " & oSummProps.SlideCount
   lstSummProps.AddItem "NoteCount:    " & oSummProps.NoteCount
   lstSummProps.AddItem "HiddenSlides: " & oSummProps.HiddenSlideCount
   lstSummProps.AddItem "MultimediaClips: " & oSummProps.MultimediaClipCount
   lstSummProps.AddItem "DateCreated:  " & oSummProps.DateCreated
   lstSummProps.AddItem "DateLastPrinted: " & oSummProps.DateLastPrinted
   lstSummProps.AddItem "DateLastSaved: " & oSummProps.DateLastSaved
   lstSummProps.AddItem "TotalEditingTime (mins): " & oSummProps.TotalEditTime
   lstSummProps.AddItem "Template:    " & oSummProps.Template
   lstSummProps.AddItem "Revision:    " & oSummProps.RevisionNumber
   lstSummProps.AddItem "IsShared:    " & oSummProps.SharedDocument
   
 ' Add a few other items that pertain to OLE files only...
   If m_oDocumentProps.IsOleFile Then
      lstSummProps.AddItem "CLSID:      " & m_oDocumentProps.CLSID
      lstSummProps.AddItem "ProgID:     " & m_oDocumentProps.ProgId
      lstSummProps.AddItem "OleFormat:  " & m_oDocumentProps.OleDocumentFormat
      lstSummProps.AddItem "OleType:    " & m_oDocumentProps.OleDocumentType
   End If
   
' We'll get the thumnail image of the document (if available).
   chkShowPreview.Enabled = False
   If Not IsEmpty(oSummProps.Thumbnail) Then
      Set oPicDisp = oSummProps.Thumbnail
      If Not oPicDisp Is Nothing Then
         Set picPreview.Picture = oPicDisp
         chkShowPreview.Enabled = True
      End If
   End If

 ' Now load the custom properties. We use a simple list box
 ' to display each item, and append the name, value, and type
 ' in a string. This is for simplicity sake...
   For Each oCustProp In m_oDocumentProps.CustomProperties
      sTmp = oCustProp.Name & ": " & CStr(oCustProp.Value)
      sTmp = sTmp & "   [" & CustTypeName(oCustProp.Type) & "]"
      lstCustProps.AddItem sTmp
   Next
   
 ' Enable/Disable text items if file is open read only...
   Call EnableItems(Not m_oDocumentProps.IsReadOnly)

 ' The operation was successful.
   OpenDocumentProperties = True
Exit Function

Err_Trap:
 ' Trap errors returned from componenet and display...
   MsgBox "Error: " & Err.Description, vbCritical, "Err: " & CStr(Err.Number)
End Function

'*********************************************************************
' CloseCurrentDocument -- Closes the documents and prompts user to
'    save the changes made (if any).
'*********************************************************************
Private Sub CloseCurrentDocument(bPromptToSaveIfDirty As Boolean)

 ' If changes where made, ask user if they want to save...
   If bPromptToSaveIfDirty And m_oDocumentProps.IsDirty Then
      If MsgBox("Would you like to save the changes made?", vbQuestion Or vbYesNo) = vbYes Then
         m_oDocumentProps.Save
      End If
   End If

 ' Close before exit...
   m_oDocumentProps.Close
   
End Sub

'*********************************************************************
' Add & Remove custom properties to the open file.
'*********************************************************************
Private Sub cmdCustAdd_Click()
   Dim oCustProp As DSOFile.CustomProperty
   Dim sName As String, sTmp As String
   Dim sValueText As String
   Dim vValue As Variant
   Dim lType As Long
   
   On Error Resume Next
   sName = txtCustName.Text
   sValueText = txtCustValue.Text

 ' We can't add a custom property unless we have a valid name
 ' and value, so we do a quick check now to avoid error later on.
   If ((sName = "") Or (sValueText = "")) Then Exit Sub
   
 ' Convert the Text string to a VARIANT of the type
 ' specified in the drop down list.
   lType = lstCustType.ListIndex + 1
   Select Case lType
      Case 2
         vValue = CLng(sValueText)
      Case 3
         vValue = CDbl(sValueText)
      Case 4
         vValue = CBool(sValueText)
      Case 5
         vValue = CDate(sValueText)
      Case Else
         vValue = sValueText
   End Select
   
 ' Add the property...
   Set oCustProp = m_oDocumentProps.CustomProperties.Add(sName, vValue)
   If Err Then
    ' If an error occurs, it's most likely because the
    ' the property name already exists...
      MsgBox "The item could not be added:" & vbCrLf & Err.Description
      Err.Clear
   Else
    ' Add item to our list box...
      sTmp = oCustProp.Name & ": " & CStr(oCustProp.Value) & "    ["
      sTmp = sTmp & CustTypeName(oCustProp.Type) & "]"
      lstCustProps.AddItem sTmp
      
      txtCustName.Text = ""
      txtCustValue.Text = ""
   End If
   
End Sub

Private Sub cmdCustRemove_Click()
   Dim oRmProp As DSOFile.CustomProperty
   Dim sName As String, sTmp As String
   
   On Error Resume Next
   sTmp = lstCustProps.List(lstCustProps.ListIndex)
   sName = Left(sTmp, InStr(sTmp, ":") - 1)
   
 ' Set a reference to the custom property we want and
 ' then call remove...
   Set oRmProp = m_oDocumentProps.CustomProperties.Item(sName)
   oRmProp.Remove
   Set oRmProp = Nothing
   
   lstCustProps.RemoveItem lstCustProps.ListIndex
   cmdCustRemove.Enabled = False
End Sub

'*********************************************************************
' Text Box Validation
'
' This code checks the values of the text box to see if it should be
' updated and if so, it changes the value. This dirties the file in
' memory, but will not change the actual file on disk until Save is
' called (which is done on close if user selects yes to dialog).
'
'*********************************************************************
Private Sub txtAuthor_Validate(Cancel As Boolean)
   Dim oSummProps As DSOFile.SummaryProperties
   On Error Resume Next
   Set oSummProps = m_oDocumentProps.SummaryProperties
   If (txtAuthor.Text <> oSummProps.Author) Then
      oSummProps.Author = txtAuthor.Text
   End If
End Sub

Private Sub txtComments_Validate(Cancel As Boolean)
   Dim oSummProps As DSOFile.SummaryProperties
   On Error Resume Next
   Set oSummProps = m_oDocumentProps.SummaryProperties
   If (txtComments.Text <> oSummProps.Comments) Then
      oSummProps.Comments = txtComments.Text
   End If
End Sub

Private Sub txtTitle_Validate(Cancel As Boolean)
   Dim oSummProps As DSOFile.SummaryProperties
   On Error Resume Next
   Set oSummProps = m_oDocumentProps.SummaryProperties
   If (txtTitle.Text <> oSummProps.Title) Then
      oSummProps.Title = txtTitle.Text
   End If
End Sub

Private Sub lstCustProps_GotFocus()
   If lstCustProps.ListCount <> 0 Then cmdCustRemove.Enabled = True
End Sub

'*********************************************************************
' Helper Functions
'*********************************************************************
Private Function CustTypeName(lType As Long) As String
 ' This function simply maps string names to the
 ' VARIANT type of a custom property.
   Select Case lType
      Case 1
         CustTypeName = "String"
      Case 2
         CustTypeName = "Long"
      Case 3
         CustTypeName = "Double"
      Case 4
         CustTypeName = "Boolean"
      Case 5
         CustTypeName = "Date"
      Case Else
         CustTypeName = "Unknown"
   End Select
End Function

Private Sub EnableItems(bEnable As Boolean)
   txtTitle.Enabled = bEnable
   txtAuthor.Enabled = bEnable
   txtComments.Enabled = bEnable
   txtCustName.Enabled = bEnable
   txtCustValue.Enabled = bEnable
   lstCustType.Enabled = bEnable
   lstCustProps.Enabled = bEnable
   cmdCustAdd.Enabled = bEnable
End Sub


