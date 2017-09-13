/***************************************************************************
 * DSOFILE.H
 *
 * Developer Support OLE Document Property Reader Sample
 *
 *  Copyright (c)1999-2001 Microsoft Corporation, All Rights Reserved
 *  Microsoft Product Support Services, Developer Support
 *
 *  You have a royalty-free right to use, modify, reproduce and distribute
 *  this sample application, and/or any modified version, in any way you
 *  find useful, provided that you agree that Microsoft has no warranty,
 *  obligations or liability for the code or information provided herein.
 *
 *  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 *  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 *  WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 *
 ***************************************************************************/
#ifndef DS_DSOFILE_H 
#define DS_DSOFILE_H

////////////////////////////////////////////////////////////////////
// Built for Windows 98, NT4, ME, 2000, XP, and 2003 (32-bit DCOM)
//
#define WIN32_LEAN_AND_MEAN
#define WINVER 0x0400

////////////////////////////////////////////////////////////////////
// Standard include files (turn off some warnings)
//
#include <windows.h>
#include <ole2.h>
#include "version.h"
#include ".\lib\dsofilelib.h"
#include ".\res\resource.h"

#pragma warning(disable: 4100) // unreferenced formal parameter (in OLE this is common)
#pragma warning(disable: 4103) // pragma pack
#pragma warning(disable: 4127) // constant expression
#pragma warning(disable: 4146) // unary minus operator applied to unsigned type, result still unsigned
#pragma warning(disable: 4201) // nameless unions are part of C++
#pragma warning(disable: 4310) // cast truncates constant value
#pragma warning(disable: 4505) // unreferenced local function has been removed
#pragma warning(disable: 4710) // function couldn't be inlined
#pragma warning(disable: 4786) // identifier was truncated in the debug information

////////////////////////////////////////////////////////////////////
// Module Gloabls and Accessors...
//
extern HINSTANCE           v_hModule;
extern ULONG               v_cLocks;
extern CRITICAL_SECTION    v_csSynch;
extern HANDLE              v_hPrivateHeap;
extern BOOL                v_fRunningOnNT;

#define DllModuleHandle()  ((HMODULE)v_hModule)
#define DllAddRef()  InterlockedIncrement((LPLONG)&v_cLocks)
#define DllRelease() InterlockedDecrement((LPLONG)&v_cLocks)
#define EnterCritical() EnterCriticalSection(&v_csSynch)
#define LeaveCritical() LeaveCriticalSection(&v_csSynch)

////////////////////////////////////////////////////////////////////
// StgOpenStorageEx (for Windows 2000/XP NTFS Non-OLE Files)
//
typedef HRESULT (CALLBACK* PFN_STGOPENSTGEX)(WCHAR*, DWORD, DWORD, DWORD, void*, void*, REFIID riid, void **);
extern PFN_STGOPENSTGEX v_pfnStgOpenStorageEx;

////////////////////////////////////////////////////////////////////////
// Fixed Win32 Errors as HRESULTs
//
#define E_WIN32_ACCESSVIOLATION   0x800701E7   //HRESULT_FROM_WIN32(ERROR_INVALID_ADDRESS)
#define E_WIN32_BUFFERTOOSMALL    0x8007007A   //HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER)

////////////////////////////////////////////////////////////////////
// Custom Errors - we support a very limited set of custom error messages
//
#define DSO_E_ERR_BASE              0x80041100
#define DSO_E_DOCUMENTNOTOPEN       0x80041101   // "You must open a document to perform the action requested."
#define DSO_E_DOCUMENTOPENED        0x80041102   // "You must close the current document before opening a new one in the same object."
#define DSO_E_DOCUMENTLOCKED        0x80041103   // "The document is in use by another program and cannot be opened for read-write access."
#define DSO_E_NODOCUMENTPROPS       0x80041104   // "The document is not an OLE file, and does not support extended document properties."
#define DSO_E_DOCUMENTREADONLY      0x80041105   // "The command is not available because document was opened in read-only mode."
#define DSO_E_MUSTHAVESTORAGE       0x80041106   // "The command is available for OLE Structured Storage files only."
#define DSO_E_INVALIDOBJECT         0x80041107   // "The object is not connected to the document (it was removed or the document was closed)."
#define DSO_E_INVALIDPROPSET        0x80041108   // "Cannot access property because the set it belongs to does not exist."
#define DSO_E_INVALIDINDEX          0x80041109   // "The property requested does not exist in the collection."
#define DSO_E_ITEMEXISTS            0x8004110A   // "An item by that already exists in the collection."
#define DSO_E_ERR_MAX               0x8004110B

////////////////////////////////////////////////////////////////////
// Core PropertySet Functions (Central Part for Read/Write of Properties)
//
STDAPI DsoOpenPropertyStorage(IPropertySetStorage* pPropSS, REFFMTID fmtid, BOOL fReadOnly, DWORD dwFlags, IPropertyStorage** ppPropStg);
STDAPI DsoReadProperty(IPropertyStorage* pPropStg, PROPSPEC spc, WORD wCodePage, VARIANT* pvtResult);
STDAPI DsoWriteProperty(IPropertyStorage* pPropStg, PROPSPEC spc, WORD wCodePage, VARIANT* pvtValue);
STDAPI DsoLoadPropertySetList(IPropertyStorage *pPropStg, WORD *pwCodePage, class CDsoDocProperty** pplist);
STDAPI DsoSavePropertySetList(IPropertyStorage *pPropStg, WORD wCodePage, class CDsoDocProperty* plist, ULONG *pcSavedItems);

////////////////////////////////////////////////////////////////////
// Object Classes (Core Automation Implementation)
//
////////////////////////////////////////////////////////////////////
// CDsoDocProperty - Basic object for single property.
// 
class CDsoDocProperty : public CustomProperty, public ISupportErrorInfo
{
public:
    CDsoDocProperty();
    ~CDsoDocProperty(void);

 // IUnknown Implementation
    STDMETHODIMP         QueryInterface(REFIID riid, void ** ppv);
    STDMETHODIMP_(ULONG) AddRef(void);
    STDMETHODIMP_(ULONG) Release(void);

 // IDispatch Implementation
	STDMETHODIMP GetTypeInfoCount(UINT* pctinfo);
	STDMETHODIMP GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo);
	STDMETHODIMP GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId);
	STDMETHODIMP Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr);

 // CustomProperty Implementation
	STDMETHODIMP get_Name(BSTR *pbstrName);
	STDMETHODIMP get_Type(dsoFilePropertyType *dsoType);
	STDMETHODIMP get_Value(VARIANT *pvValue);
	STDMETHODIMP put_Value(VARIANT *pvValue);
	STDMETHODIMP Remove();

 // ISupportErrorInfo Implementation
    STDMETHODIMP InterfaceSupportsErrorInfo(REFIID riid)
		{return ((riid == IID_CustomProperty) ? S_OK : E_FAIL);}

 // Internal Functions
	STDMETHODIMP InitProperty(BSTR bstrName, PROPID propid, VARIANT* pvData, BOOL fNewItem, CDsoDocProperty* pPreviousItem);
    STDMETHODIMP_(CDsoDocProperty*) GetNextProperty(){return m_pNextItem;}
    STDMETHODIMP_(CDsoDocProperty*) AppendLink(CDsoDocProperty* pLinkItem);
    STDMETHODIMP_(PROPID) GetID(){return m_ulPropID;}
    STDMETHODIMP_(VARIANT*) GetDataPtr(){return &m_vValue;}
    STDMETHODIMP_(BOOL) IsRemoved(){return m_fRemovedItem;}
    STDMETHODIMP_(BOOL) IsDirty(){return (m_fModified && !(m_fDeadObj));}
    STDMETHODIMP_(BOOL) IsNewItem(){return m_fNewItem;}
    STDMETHODIMP_(void) Renew(){m_fRemovedItem = FALSE;}
    STDMETHODIMP_(void) Disconnect(){m_fDeadObj = TRUE; Release();}
    STDMETHODIMP_(void) OnSaveComplete(){m_fModified = FALSE; m_fNewItem = FALSE; m_fRemovedItem = FALSE;}
    STDMETHODIMP_(void) OnRemoveComplete(){m_fModified = FALSE; m_fNewItem = TRUE; m_fRemovedItem = TRUE;}

	static CDsoDocProperty* CreateObject(BSTR bstrName, PROPID propid, VARIANT* pvData, BOOL fNewItem, CDsoDocProperty* pPreviousItem);

private:
    ULONG        m_cRef;		        // Reference count
    ITypeInfo*   m_ptiDispType;         // ITypeInfo Pointer (IDispatch Impl)
    EXCEPINFO*   m_pDispExcep;          // EXCEPINFO Pointer (IDispatch Impl)
	BSTR		 m_bstrName;            // Property Name
    PROPID       m_ulPropID;            // Property ID
	VARIANT		 m_vValue;              // Property Value
    BOOL         m_fModified;           // Do we need to update item?
    BOOL         m_fExternal;           // Does object have external ref count?
	BOOL         m_fDeadObj;            // Is object still valid?
    BOOL         m_fNewItem;            // Is item added to list after load?
    BOOL         m_fRemovedItem;        // Is item marked for delete?
    CDsoDocProperty* m_pNextItem;       // Items are linked in single link list (stores previous item)
};

////////////////////////////////////////////////////////////////////
// CDsoCustomProperties -  Collection Class For Custom Properties
//  (FMTID_UserDefinedProperties)
//
class CDsoCustomProperties :  public CustomProperties, public IEnumVARIANT, public ISupportErrorInfo
{
public:
    CDsoCustomProperties();
    ~CDsoCustomProperties(void);

 // IUnknown Implementation
    STDMETHODIMP         QueryInterface(REFIID riid, void ** ppv);
    STDMETHODIMP_(ULONG) AddRef(void);
    STDMETHODIMP_(ULONG) Release(void);

 // IDispatch Implementation
	STDMETHODIMP GetTypeInfoCount(UINT* pctinfo);
	STDMETHODIMP GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo);
	STDMETHODIMP GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId);
    STDMETHODIMP Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr);

 // CustomProperties Implementation
	STDMETHODIMP get_Count(long* lCount);
	STDMETHODIMP Add(BSTR sPropName, VARIANT* Value, CustomProperty** ppDocProperty);
	STDMETHODIMP get_Item(VARIANT Index, CustomProperty** ppDocProperty);
	STDMETHODIMP get__NewEnum(IUnknown** ppunk);

 // IEnumVARIANT Implementation 
    STDMETHODIMP  Next(ULONG cElements, VARIANT* pvar, ULONG* pcElementFetched);
    STDMETHODIMP  Skip(ULONG cElements);
    STDMETHODIMP  Reset();
    STDMETHODIMP  Clone(IEnumVARIANT** ppenum);

 // ISupportErrorInfo Implementation
    STDMETHODIMP InterfaceSupportsErrorInfo(REFIID riid)
		{return ((riid == IID_CustomProperties) ? S_OK : E_FAIL);}

 // Internal Functions
	STDMETHODIMP LoadProperties(IPropertySetStorage* pPropSS, BOOL fIsReadOnly, dsoFileOpenOptions dwFlags);
    STDMETHODIMP_(CDsoDocProperty*) GetPropertyFromList(CDsoDocProperty* plist, LPCWSTR pwszName, BOOL fAppendNew);
    STDMETHODIMP_(CDsoDocProperty*) GetPropertyByCount(CDsoDocProperty* plist, ULONG cItem);
	STDMETHODIMP SaveProperties(BOOL fCommitChanges);
    STDMETHODIMP_(BOOL) FIsDirty();
    STDMETHODIMP_(void) Disconnect();

private:
    ULONG                   m_cRef;		    // Reference count
    ITypeInfo*              m_ptiDispType;  // ITypeInfo Pointer (IDispatch Impl)
    EXCEPINFO*              m_pDispExcep;   // EXCEPINFO Pointer (IDispatch Impl)
	IPropertySetStorage*    m_pPropSetStg;  // Property Set Storage
    dsoFileOpenOptions      m_dwFlags;      // Open Flags
	BOOL		            m_fReadOnly;    // Should be read-only?
    BOOL                    m_fExternal;    // Does object have external ref count?
	BOOL                    m_fDeadObj;     // Is object still connected?
    WORD                    m_wCodePage;    // Code Page for MBCS/Unicode translation
    CDsoDocProperty*        m_pCustPropList;// List of Custom Properties
    CDsoDocProperty*        m_pCurrentProp; // Current Item (for IEnumVariant step)
};

////////////////////////////////////////////////////////////////////
// CDsoSummaryProperties - Collection Class For Summary Properties
//   (FMTID_SummaryInformation and FMTID_DocSummaryInformation)
//
class CDsoSummaryProperties :  public SummaryProperties, public ISupportErrorInfo
{
public:
    CDsoSummaryProperties();
    ~CDsoSummaryProperties(void);

 // IUnknown Implementation
    STDMETHODIMP         QueryInterface(REFIID riid, void ** ppv);
    STDMETHODIMP_(ULONG) AddRef(void);
    STDMETHODIMP_(ULONG) Release(void);

 // IDispatch Implementation
	STDMETHODIMP GetTypeInfoCount(UINT* pctinfo);
	STDMETHODIMP GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo);
	STDMETHODIMP GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId);
    STDMETHODIMP Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr);

 // SummaryProperties Implementation
   // FMTID_SummaryInformation Properties...
	STDMETHODIMP get_Title(BSTR* pbstrTitle);
	STDMETHODIMP put_Title(BSTR bstrTitle);
	STDMETHODIMP get_Subject(BSTR* pbstrSubject);
	STDMETHODIMP put_Subject(BSTR bstrSubject);
	STDMETHODIMP get_Author(BSTR* pbstrAuthor);
	STDMETHODIMP put_Author(BSTR bstrAuthor);
	STDMETHODIMP get_Keywords(BSTR* pbstrKeywords);
	STDMETHODIMP put_Keywords(BSTR bstrKeywords);
	STDMETHODIMP get_Comments(BSTR* pbstrComments);
	STDMETHODIMP put_Comments(BSTR bstrComments);
	STDMETHODIMP get_Template(BSTR* pbstrTemplate);
	STDMETHODIMP get_LastSavedBy(BSTR* pbstrLastSavedBy);
	STDMETHODIMP put_LastSavedBy(BSTR bstrLastSavedBy);
	STDMETHODIMP get_RevisionNumber(BSTR* pbstrRevisionNumber);
	STDMETHODIMP get_TotalEditTime(long* plTotalEditTime);
	STDMETHODIMP get_DateLastPrinted(VARIANT* pdtDateLastPrinted);
	STDMETHODIMP get_DateCreated(VARIANT* pdtDateCreated);
	STDMETHODIMP get_DateLastSaved(VARIANT* pdtDateLastSaved);
	STDMETHODIMP get_PageCount(long* plPageCount);
	STDMETHODIMP get_WordCount(long* plWordCount);
	STDMETHODIMP get_CharacterCount(long* plCharacterCount);
	STDMETHODIMP get_Thumbnail(VARIANT* pvtThumbnail);
	STDMETHODIMP get_ApplicationName(BSTR* pbstrAppName);
	STDMETHODIMP get_DocumentSecurity(long* plDocSecurity);

   // FMTID_DocSummaryInformation Properties...
	STDMETHODIMP get_Category(BSTR* pbstrCategory);
	STDMETHODIMP put_Category(BSTR bstrCategory);
	STDMETHODIMP get_PresentationFormat(BSTR* pbstrPresFormat);
	STDMETHODIMP get_ByteCount(long* plByteCount);
	STDMETHODIMP get_LineCount(long* plLineCount);
	STDMETHODIMP get_ParagraphCount(long* plParagraphCount);
	STDMETHODIMP get_SlideCount(long* plSlideCount);
	STDMETHODIMP get_NoteCount(long* plNoteCount);
	STDMETHODIMP get_HiddenSlideCount(long* plHiddenSlideCount);
	STDMETHODIMP get_MultimediaClipCount(long* plMultimediaClipCount);
	STDMETHODIMP get_Manager(BSTR* pbstrManager);
	STDMETHODIMP put_Manager(BSTR bstrManager);
	STDMETHODIMP get_Company(BSTR* pbstrCompany);
	STDMETHODIMP put_Company(BSTR bstrCompany);
	STDMETHODIMP get_CharacterCountWithSpaces(long* plCharCountWithSpaces);
	STDMETHODIMP get_SharedDocument(VARIANT_BOOL* pbSharedDocument);
	STDMETHODIMP get_Version(BSTR* pbstrVersion);
	STDMETHODIMP get_DigitalSignature(VARIANT* pvtDigSig);

 // ISupportErrorInfo Implementation
    STDMETHODIMP InterfaceSupportsErrorInfo(REFIID riid)
		{return ((riid == IID_SummaryProperties) ? S_OK : E_FAIL);}

 // Internal Functions
	STDMETHODIMP LoadProperties(IPropertySetStorage* pPropSS, BOOL fIsReadOnly, dsoFileOpenOptions dwFlags);
	STDMETHODIMP ReadProperty(CDsoDocProperty* pPropList, PROPID pid, VARTYPE vt, void** ppv);
	STDMETHODIMP WriteProperty(CDsoDocProperty** ppPropList, PROPID pid, VARTYPE vt, void* pv);
    STDMETHODIMP_(CDsoDocProperty*) GetPropertyFromList(CDsoDocProperty* plist, PROPID id, BOOL fAppendNew);
	STDMETHODIMP SaveProperties(BOOL fCommitChanges);
    STDMETHODIMP_(BOOL) FIsDirty();
    STDMETHODIMP_(void) Disconnect();

private:
    ULONG                   m_cRef;		    // Reference count
    ITypeInfo*              m_ptiDispType;  // ITypeInfo Pointer (IDispatch Impl)
    EXCEPINFO*              m_pDispExcep;   // EXCEPINFO Pointer (IDispatch Impl)
	IPropertySetStorage*    m_pPropSetStg;  // Property Set Storage
    dsoFileOpenOptions      m_dwFlags;      // Open Flags
	BOOL		            m_fReadOnly;    // Should be read-only?
    BOOL                    m_fExternal;    // Does object have external ref count?
	BOOL                    m_fDeadObj;     // Is object still connected?
    CDsoDocProperty*        m_pSummPropList;// List of Summary Properties
    CDsoDocProperty*        m_pDocPropList; // List of Doc Summary Properties
    WORD                    m_wCodePageSI;  // Code Page for SummPropList
    WORD                    m_wCodePageDSI; // Code Page for DocPropList
};

////////////////////////////////////////////////////////////////////
// CDsoDocumentProperties - Main Object for Document
//
class CDsoDocumentProperties :  public _OleDocumentProperties, public ISupportErrorInfo
{
public:
    CDsoDocumentProperties();
    ~CDsoDocumentProperties(void);

 // IUnknown Implementation
    STDMETHODIMP         QueryInterface(REFIID riid, void ** ppv);
    STDMETHODIMP_(ULONG) AddRef(void);
    STDMETHODIMP_(ULONG) Release(void);

 // IDispatch Implementation
	STDMETHODIMP GetTypeInfoCount(UINT* pctinfo);
	STDMETHODIMP GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo);
	STDMETHODIMP GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId);
    STDMETHODIMP Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr);

 // _OleDocumentProperties Implementation
	STDMETHODIMP Open(BSTR sFileName, VARIANT_BOOL ReadOnly, dsoFileOpenOptions Options);
	STDMETHODIMP Close(VARIANT_BOOL SaveBeforeClose);
	STDMETHODIMP get_IsReadOnly(VARIANT_BOOL* pbReadOnly);
	STDMETHODIMP get_IsDirty(VARIANT_BOOL* pbDirty);
	STDMETHODIMP Save();
	STDMETHODIMP get_SummaryProperties(SummaryProperties** ppSummaryProperties);
	STDMETHODIMP get_CustomProperties(CustomProperties** ppCustomProperties);
	STDMETHODIMP get_Icon(IDispatch** ppicIcon);
	STDMETHODIMP get_Name(BSTR* pbstrName);
	STDMETHODIMP get_Path(BSTR* pbstrPath);
	STDMETHODIMP get_IsOleFile(VARIANT_BOOL* pIsOleFile);
	STDMETHODIMP get_CLSID(BSTR* pbstrCLSID);
	STDMETHODIMP get_ProgID(BSTR* pbstrProgID);
	STDMETHODIMP get_OleDocumentFormat(BSTR* pbstrFormat);
	STDMETHODIMP get_OleDocumentType(BSTR* pbstrType);

 // ISupportErrorInfo Implementation
    STDMETHODIMP InterfaceSupportsErrorInfo(REFIID riid)
		{return ((riid == IID__OleDocumentProperties) ? S_OK : E_FAIL);}

 // Internal Functions
    STDMETHODIMP InitializeNewInstance(){return S_OK;} // (for future use?)

private:
    ULONG                   m_cRef;		    // Reference count
    ITypeInfo*              m_ptiDispType;  // ITypeInfo Pointer (IDispatch Impl)
    EXCEPINFO*              m_pDispExcep;   // EXCEPINFO Pointer (IDispatch Impl)
	BSTR				    m_bstrFileName; // Filename of open document
	ULONG				    m_cFilePartIdx; // Path/Name Index
    IStorage*				m_pStorage;     // IStorage document pointer
	IPropertySetStorage*    m_pPropSetStg;  // Property Set Storage
    dsoFileOpenOptions      m_dwFlags;      // Open Flags
	BOOL		            m_fReadOnly;    // Should be read-only?
    WORD                    m_wCodePage;    // Code Page for MBCS/Unicode translation
    CDsoSummaryProperties  *m_pSummProps;   // Summary Properties Object
    CDsoCustomProperties   *m_pCustomProps; // Custom Properties Object
	IPersistFile           *m_pPrstFile;
	DWORD                   m_dwAttributes;
	BOOL                    m_fResetAttrib;
};


////////////////////////////////////////////////////////////////////
// CDsoDocumentClassFactory - OleDocumentProperties Class Factory
//
class CDsoDocumentClassFactory : public IClassFactory
{
public:
    CDsoDocumentClassFactory(): m_cRef(0){}
    ~CDsoDocumentClassFactory(void){}

 // IUnknown Implementation
    STDMETHODIMP         QueryInterface(REFIID riid, void ** ppv);
    STDMETHODIMP_(ULONG) AddRef(void);
    STDMETHODIMP_(ULONG) Release(void);

 // IClassFactory Implementation
    STDMETHODIMP  CreateInstance(LPUNKNOWN punk, REFIID riid, void** ppv);
    STDMETHODIMP  LockServer(BOOL fLock);

private:
    ULONG          m_cRef;          // Reference count
};


////////////////////////////////////////////////////////////////////////
// Internal Global Functions
//
////////////////////////////////////////////////////////////////////////
// IDispatch/ITypeInfo Handler Functions
//
STDAPI DsoGetTypeInfoEx(REFGUID rlibid, LCID lcid, WORD wVerMaj, WORD wVerMin, HMODULE hResource, REFGUID rguid, ITypeInfo** ppti);
STDAPI DsoGetTypeInfo(REFGUID rguid, ITypeInfo** ppti);
STDAPI DsoAutoDispInvoke(LPDISPATCH pdisp, LPOLESTR pwszname, DISPID dspid,	WORD wflags, DWORD cargs, VARIANT* rgargs, VARIANT* pvtret);
STDAPI DsoReportError(HRESULT hr, LPWSTR pwszCustomMessage, EXCEPINFO* peiDispEx);

////////////////////////////////////////////////////////////////////////
// Heap Allocation (Uses private Win32 Heap)
//
STDAPI_(LPVOID) DsoMemAlloc(DWORD cbSize);
STDAPI_(void)   DsoMemFree(LPVOID ptr);

// Override new/delete to use our task allocator
// (removing CRT dependency will improve code performance and size)...
void * _cdecl operator new(size_t size);
void  _cdecl operator delete(void *ptr);

////////////////////////////////////////////////////////////////////////
// String Manipulation Functions
//
STDAPI DsoConvertToUnicodeEx(LPCSTR pszMbcsString, DWORD cbMbcsLen, LPWSTR pwszUnicode, DWORD cbUniLen, WORD wCodePage);
STDAPI DsoConvertToMBCSEx(LPCWSTR pwszUnicodeString, DWORD cbUniLen, LPSTR pszMbcsString, DWORD cbMbcsLen, WORD wCodePage);
STDAPI_(LPWSTR) DsoConvertToCoTaskMemStr(BSTR bstrString);
STDAPI_(LPSTR)  DsoConvertToMBCS(LPCWSTR pwszUnicodeString, WORD wCodePage);
STDAPI_(BSTR)   DsoConvertToBSTR(LPCSTR pszAnsiString, WORD wCodePage);
STDAPI_(UINT)   DsoCompareStrings(LPCWSTR pwsz1, LPCWSTR pwsz2);

////////////////////////////////////////////////////////////////////////
// Unicode Win32 API wrappers (handles Unicode/ANSI convert for Win98/ME)
//
STDAPI_(BOOL) FFindQualifiedFileName(LPCWSTR pwszFile, LPWSTR pwszPath, ULONG *pcPathIdx);
STDAPI_(BOOL) FGetModuleFileName(HMODULE hModule, WCHAR** wzFileName);
STDAPI_(BOOL) FSetRegKeyValue(HKEY hk, WCHAR* pwsz);
STDAPI_(BOOL) FGetIconForFile(LPCWSTR pwszFile, HICON *pico);

////////////////////////////////////////////////////////////////////////
// Attribute correction (to handle change of read-only flag)
//
STDAPI_(DWORD) DsoGetFileAttrib(LPCWSTR pwszFile);
STDAPI_(BOOL)  DsoSetFileAttrib(LPCWSTR pwszFile, DWORD dwAttrib);

////////////////////////////////////////////////////////////////////////
// Shell Metadata Handler Lookup
//
STDAPI DsoGetMetaHandler(LPCWSTR pwszFile, LPCLSID lpClsid);


////////////////////////////////////////////////////////////////////////
// Property Identifiers for Office Document Properties...
//
#ifndef PID_DICTIONARY // User Defined Properties (Dictionary based)
#define PID_DICTIONARY			0x00000000L
#define PID_CODEPAGE			0x00000001L
#endif
#ifndef PIDSI_TITLE    // Summary Information Properties (All OLE Files)
#define PIDSI_TITLE             0x00000002L  // VT_LPSTR
#define PIDSI_SUBJECT           0x00000003L  // VT_LPSTR
#define PIDSI_AUTHOR            0x00000004L  // VT_LPSTR
#define PIDSI_KEYWORDS          0x00000005L  // VT_LPSTR
#define PIDSI_COMMENTS          0x00000006L  // VT_LPSTR
#define PIDSI_TEMPLATE          0x00000007L  // VT_LPSTR
#define PIDSI_LASTAUTHOR        0x00000008L  // VT_LPSTR
#define PIDSI_REVNUMBER         0x00000009L  // VT_LPSTR
#define PIDSI_EDITTIME          0x0000000aL  // VT_FILETIME (UTC)
#define PIDSI_LASTPRINTED       0x0000000bL  // VT_FILETIME (UTC)
#define PIDSI_CREATE_DTM        0x0000000cL  // VT_FILETIME (UTC)
#define PIDSI_LASTSAVE_DTM      0x0000000dL  // VT_FILETIME (UTC)
#define PIDSI_PAGECOUNT         0x0000000eL  // VT_I4
#define PIDSI_WORDCOUNT         0x0000000fL  // VT_I4
#define PIDSI_CHARCOUNT         0x00000010L  // VT_I4
#define PIDSI_THUMBNAIL         0x00000011L  // VT_CF
#define PIDSI_APPNAME           0x00000012L  // VT_LPSTR
#define PIDSI_DOC_SECURITY      0x00000013L  // VT_I4
#endif
#ifndef PID_CATEGORY   // Document Summary Information Properties (Office 97+ Files)
#define PID_CATEGORY            0x00000002L	 // VT_LPSTR
#define PID_PRESFORMAT          0x00000003L	 // VT_LPSTR
#define PID_BYTECOUNT           0x00000004L	 // VT_I4
#define PID_LINECOUNT           0x00000005L	 // VT_I4
#define PID_PARACOUNT           0x00000006L	 // VT_I4
#define PID_SLIDECOUNT          0x00000007L	 // VT_I4
#define PID_NOTECOUNT           0x00000008L	 // VT_I4
#define PID_HIDDENCOUNT         0x00000009L	 // VT_I4
#define PID_MMCLIPCOUNT         0x0000000aL	 // VT_I4
#define PID_SCALE               0x0000000bL	 // VT_BOOL
#define PID_HEADINGPAIR         0x0000000cL	 // VT_VECTOR | VT_VARIANT
#define PID_DOCPARTS            0x0000000dL	 // VT_VECTOR | VT_LPSTR
#define PID_MANAGER             0x0000000eL	 // VT_LPSTR
#define PID_COMPANY             0x0000000fL	 // VT_LPSTR
#define PID_LINKSDIRTY          0x00000010L	 // VT_BOOL
#define PID_CCHWITHSPACES 		0x00000011L	 // VT_I4
#define PID_GUID				0x00000012L  // VT_LPSTR -- RESERVED, no longer used
#define PID_SHAREDDOC			0x00000013L	 // VT_BOOL
#define PID_LINKBASE			0x00000014L  // VT_LPSTR -- RESERVED, no longer used
#define PID_HLINKS              0x00000015L	 // VT_VECTOR | VT_VARIANT -- RESERVED, no longer used
#define PID_HLINKSCHANGED       0x00000016L	 // VT_BOOL
#define PID_VERSION				0x00000017L	 // VT_I4
#define PID_DIGSIG              0x00000018L  // VT_BLOB
#endif

////////////////////////////////////////////////////////////////////////
// Common macros -- Used to make code more readable.
//
#define SEH_TRY           __try {
#define SEH_EXCEPT(h)     } __except(GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION){h = E_WIN32_ACCESSVIOLATION;}
#define SEH_EXCEPT_NULL   } __except(GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION){}
#define SEH_START_FINALLY } __finally {
#define SEH_END_FINALLY   }

#define CHECK_NULL_RETURN(var, err)      if (NULL==(var)) return (err)
#define CHECK_FLAG_RETURN(flg, err)      if (flg) return (err)
#define RETURN_ON_FAILURE(hr)            if (FAILED(hr)) { return hr; }
#define FREE_MEMPOINTER(p)               if (p) {DsoMemFree(p); p = NULL;}
#define FREE_COTASKMEM(c)                if (c) {CoTaskMemFree(c); c = NULL;}
#define FREE_BSTR(b)                     if (b) {SysFreeString(b); b = NULL;}

#define ASSIGN_INTERFACE(x, y)           if (y) {(x)=(y);(x)->AddRef();}
#define ADDREF_INTERFACE(x)              if (x) (x)->AddRef();
#define RELEASE_INTERFACE(x)             if (x) {(x)->Release(); (x) = NULL;}
#define SAFE_RELEASE_INTERFACE(x)        SEH_TRY RELEASE_INTERFACE(x) SEH_EXCEPT_NULL
#define ZOMBIE_OBJECT(x)                 if (x) {(x)->Disconnect(); (x) = NULL;}

////////////////////////////////////////////////////////////////////////
// Debug macros (tracing and asserts)
//
#ifdef _DEBUG

#define ASSERT(x)  if(!(x)) DebugBreak()
#define ODS(x)	   OutputDebugString(x)

#define TRACE1(sz, arg1) { \
	CHAR ach[MAX_PATH]; \
	wsprintf(ach, (sz), (arg1)); \
	ODS(ach); }

#define TRACE2(sz, arg1, arg2) { \
	CHAR ach[MAX_PATH]; \
	wsprintf(ach, (sz), (arg1), (arg2)); \
	ODS(ach); }

#define TRACE3(sz, arg1, arg2, arg3) { \
	CHAR ach[MAX_PATH]; \
	wsprintf(ach, (sz), (arg1), (arg2), (arg3)); \
	ODS(ach); }

#else // !defined(_DEBUG)

#define ASSERT(x)
#define ODS(x) 
#define TRACE1(sz, arg1)
#define TRACE2(sz, arg1, arg2)
#define TRACE3(sz, arg1, arg2, arg3)
#define TRACE_LPRECT(sz, lprc)

#endif // (_DEBUG)

#endif //DS_DSOFILE_H