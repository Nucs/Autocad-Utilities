/***************************************************************************
 * SUMMPROPS.CPP
 *
 * Implementation for SummaryProperties Object
 *
 *  Copyright (c)1999-2001 Microsoft Corporation, All Rights Reserved
 *  Written by DSO Office Integration, Microsoft Developer Support
 *
 *  THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 *  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 *  WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 *
 ***************************************************************************/
#include "dsofile.h"

////////////////////////////////////////////////////////////////////////
// CDsoSummaryProperties
//
////////////////////////////////////////////////////////////////////////
// Class Constructor/Destructor
//
CDsoSummaryProperties::CDsoSummaryProperties()
{
	ODS("CDsoSummaryProperties::CDsoSummaryProperties()\n");
    m_cRef           = 1; // Automatically has one ref...
    m_ptiDispType    = NULL;
    m_pDispExcep     = NULL;
    m_pPropSetStg    = NULL;
    m_dwFlags        = dsoOptionDefault;
    m_fReadOnly      = FALSE;
    m_fExternal      = FALSE;
    m_fDeadObj       = FALSE;
    m_pSummPropList  = NULL;
    m_pDocPropList   = NULL;
    m_wCodePageSI    = CP_ACP;
    m_wCodePageDSI   = CP_ACP;
}

CDsoSummaryProperties::~CDsoSummaryProperties(void)
{
    ODS("CDsoSummaryProperties::~CDsoSummaryProperties()\n");
    RELEASE_INTERFACE(m_ptiDispType);
}

////////////////////////////////////////////////////////////////////////
// IUnknown Implementation
//
////////////////////////////////////////////////////////////////////////
// QueryInterface
//
STDMETHODIMP CDsoSummaryProperties::QueryInterface(REFIID riid, void** ppv)
{
	HRESULT hr = S_OK;

	ODS("CDsoSummaryProperties::QueryInterface\n");
	CHECK_NULL_RETURN(ppv, E_POINTER);
	
	if ((IID_SummaryProperties == riid) || (IID_IDispatch == riid) || (IID_IUnknown == riid))
    {
		*ppv = (SummaryProperties*)this;
    }
	else if (IID_ISupportErrorInfo == riid)
    {
		*ppv = (ISupportErrorInfo*)this;
    }
	else
    {
		*ppv = NULL;
		hr = E_NOINTERFACE;
	}

    ADDREF_INTERFACE(((IUnknown*)*ppv));
	return hr;
}

////////////////////////////////////////////////////////////////////////
// AddRef
//
STDMETHODIMP_(ULONG) CDsoSummaryProperties::AddRef(void)
{
	TRACE1("CDsoSummaryProperties::AddRef - %d\n", m_cRef+1);
    if (!m_fExternal) { DllAddRef(); m_fExternal = TRUE; }
    return ++m_cRef;
}

////////////////////////////////////////////////////////////////////////
// Release
//
STDMETHODIMP_(ULONG) CDsoSummaryProperties::Release(void)
{
	ULONG ul = --m_cRef;
	TRACE1("CDsoSummaryProperties::Release - %d\n", ul);

    if (ul == 0)
	{
        ASSERT(m_fDeadObj); // We better be dead before going away!
		ODS("CDsoSummaryProperties Final Release\n");
        if (m_fExternal) DllRelease();
        delete this;
	}
    return ul;
}

////////////////////////////////////////////////////////////////////////
// IDispatch Implementation
//
////////////////////////////////////////////////////////////////////////
// GetTypeInfoCount
//
STDMETHODIMP CDsoSummaryProperties::GetTypeInfoCount(UINT* pctinfo)
{
	ODS("CDsoSummaryProperties::GetTypeInfoCount\n");
	if (pctinfo) *pctinfo = 1;
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetTypeInfo
//
STDMETHODIMP CDsoSummaryProperties::GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
{
	ODS("CDsoSummaryProperties::GetTypeInfo\n");
	CHECK_NULL_RETURN(ppTInfo, E_POINTER);
    *ppTInfo = NULL;

 // We only support default type info...
	CHECK_NULL_RETURN((iTInfo == 0), DISP_E_BADINDEX);

 // Load type info if we have not done so already
    if (NULL == m_ptiDispType)
    {
        HRESULT hr = DsoGetTypeInfo(IID_SummaryProperties, &m_ptiDispType);
        RETURN_ON_FAILURE(hr);
    }

    *ppTInfo = m_ptiDispType;
    ADDREF_INTERFACE(((IUnknown*)*ppTInfo));
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetIDsOfNames
//
STDMETHODIMP CDsoSummaryProperties::GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoSummaryProperties::GetIDsOfNames\n");
    CHECK_NULL_RETURN((IID_NULL == riid), DISP_E_UNKNOWNINTERFACE);

 // Get the type info for this dispinterface...
    hr = GetTypeInfo(0, 0, &pti);
    RETURN_ON_FAILURE(hr);

 // Ask OLE to translate the name...
    hr = pti->GetIDsOfNames(rgszNames, cNames, rgDispId);
    pti->Release();
    return hr;
}

////////////////////////////////////////////////////////////////////////
// Invoke
//
STDMETHODIMP CDsoSummaryProperties::Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoSummaryProperties::Invoke\n");
    CHECK_NULL_RETURN((IID_NULL == riid), DISP_E_UNKNOWNINTERFACE);

 // Get the type info for this dispinterface...
    hr = GetTypeInfo(0, 0, &pti);
    RETURN_ON_FAILURE(hr);

 // Store pExcepInfo (to fill-in disp excepinfo if error occurs)...
    m_pDispExcep = pExcepInfo;

 // Call the method using TypeInfo (OLE will call v-table method for us)...
    hr = pti->Invoke((PVOID)this, dispIdMember, wFlags, pDispParams, pVarResult, pExcepInfo, puArgErr);

    m_pDispExcep = NULL; // Don't need this anymore...
    pti->Release();
    return hr;
}

////////////////////////////////////////////////////////////////////////
// SummaryProperties Implementation
//
////////////////////////////////////////////////////////////////////////
// FMTID_SummaryInformation Properties...
//  
STDMETHODIMP CDsoSummaryProperties::get_Title(BSTR* pbstrTitle)
{
    ODS("CDsoSummaryProperties::get_Title\n");
	CHECK_NULL_RETURN(pbstrTitle, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_TITLE, VT_BSTR, ((void**)pbstrTitle));
}

STDMETHODIMP CDsoSummaryProperties::put_Title(BSTR bstrTitle)
{
    TRACE1("CDsoSummaryProperties::put_Title(%S)\n", bstrTitle);
	return WriteProperty(&m_pSummPropList, PIDSI_TITLE, VT_BSTR, ((void*)bstrTitle));
}

STDMETHODIMP CDsoSummaryProperties::get_Subject(BSTR* pbstrSubject)
{
    ODS("CDsoSummaryProperties::get_Subject\n");
	CHECK_NULL_RETURN(pbstrSubject, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_SUBJECT, VT_BSTR, ((void**)pbstrSubject));
}

STDMETHODIMP CDsoSummaryProperties::put_Subject(BSTR bstrSubject)
{
    TRACE1("CDsoSummaryProperties::put_Subject(%S)\n", bstrSubject);
	return WriteProperty(&m_pSummPropList, PIDSI_SUBJECT, VT_BSTR, ((void*)bstrSubject));
}

STDMETHODIMP CDsoSummaryProperties::get_Author(BSTR* pbstrAuthor)
{
    ODS("CDsoSummaryProperties::get_Author\n");
	CHECK_NULL_RETURN(pbstrAuthor, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_AUTHOR, VT_BSTR, ((void**)pbstrAuthor));
}

STDMETHODIMP CDsoSummaryProperties::put_Author(BSTR bstrAuthor)
{
    TRACE1("CDsoSummaryProperties::put_Author(%S)\n", bstrAuthor);
	return WriteProperty(&m_pSummPropList, PIDSI_AUTHOR, VT_BSTR, ((void*)bstrAuthor));
}

STDMETHODIMP CDsoSummaryProperties::get_Keywords(BSTR* pbstrKeywords)
{
    ODS("CDsoSummaryProperties::get_Keywords\n");
	CHECK_NULL_RETURN(pbstrKeywords, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_KEYWORDS, VT_BSTR, ((void**)pbstrKeywords));
}

STDMETHODIMP CDsoSummaryProperties::put_Keywords(BSTR bstrKeywords)
{
    TRACE1("CDsoSummaryProperties::put_Keywords(%S)\n", bstrKeywords);
	return WriteProperty(&m_pSummPropList, PIDSI_KEYWORDS, VT_BSTR, ((void*)bstrKeywords));
}

STDMETHODIMP CDsoSummaryProperties::get_Comments(BSTR* pbstrComments)
{
    ODS("CDsoSummaryProperties::get_Comments\n");
	CHECK_NULL_RETURN(pbstrComments, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_COMMENTS, VT_BSTR, ((void**)pbstrComments));
}

STDMETHODIMP CDsoSummaryProperties::put_Comments(BSTR bstrComments)
{
    TRACE1("CDsoSummaryProperties::put_Comments(%S)\n", bstrComments);
	return WriteProperty(&m_pSummPropList, PIDSI_COMMENTS, VT_BSTR, ((void*)bstrComments));
}

STDMETHODIMP CDsoSummaryProperties::get_Template(BSTR* pbstrTemplate)
{
    ODS("CDsoSummaryProperties::get_Template\n");
	CHECK_NULL_RETURN(pbstrTemplate, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_TEMPLATE, VT_BSTR, ((void**)pbstrTemplate));
}

STDMETHODIMP CDsoSummaryProperties::get_LastSavedBy(BSTR* pbstrLastSavedBy)
{
    ODS("CDsoSummaryProperties::get_LastSavedBy\n");
	CHECK_NULL_RETURN(pbstrLastSavedBy, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_LASTAUTHOR, VT_BSTR, ((void**)pbstrLastSavedBy));
}

STDMETHODIMP CDsoSummaryProperties::put_LastSavedBy(BSTR bstrLastSavedBy)
{
    TRACE1("CDsoSummaryProperties::put_LastSavedBy(%S)\n", bstrLastSavedBy);
	return WriteProperty(&m_pSummPropList, PIDSI_LASTAUTHOR, VT_BSTR, ((void*)bstrLastSavedBy));
}

STDMETHODIMP CDsoSummaryProperties::get_RevisionNumber(BSTR* pbstrRevisionNumber)
{
    ODS("CDsoSummaryProperties::get_RevisionNumber\n");
	CHECK_NULL_RETURN(pbstrRevisionNumber, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_REVNUMBER, VT_BSTR, ((void**)pbstrRevisionNumber));
}

STDMETHODIMP CDsoSummaryProperties::get_TotalEditTime(long* plTotalEditTime)
{
    ODS("CDsoSummaryProperties::get_TotalEditTime\n");
	CHECK_NULL_RETURN(plTotalEditTime, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_EDITTIME, VT_I4, ((void**)plTotalEditTime));
}

STDMETHODIMP CDsoSummaryProperties::get_DateLastPrinted(VARIANT* pdtDateLastPrinted)
{
    ODS("CDsoSummaryProperties::get_DateLastPrinted\n");
	CHECK_NULL_RETURN(pdtDateLastPrinted, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_LASTPRINTED, VT_DATE, ((void**)pdtDateLastPrinted));
}

STDMETHODIMP CDsoSummaryProperties::get_DateCreated(VARIANT* pdtDateCreated)
{
    ODS("CDsoSummaryProperties::get_DateCreated\n");
	CHECK_NULL_RETURN(pdtDateCreated, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_CREATE_DTM, VT_DATE, ((void**)pdtDateCreated));
}

STDMETHODIMP CDsoSummaryProperties::get_DateLastSaved(VARIANT* pdtDateLastSaved)
{
    ODS("CDsoSummaryProperties::get_DateLastSaved\n");
	CHECK_NULL_RETURN(pdtDateLastSaved, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_LASTSAVE_DTM, VT_DATE, ((void**)pdtDateLastSaved));
}

STDMETHODIMP CDsoSummaryProperties::get_PageCount(long* plPageCount)
{
    ODS("CDsoSummaryProperties::get_PageCount\n");
	CHECK_NULL_RETURN(plPageCount, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_PAGECOUNT, VT_I4, ((void**)plPageCount));
}

STDMETHODIMP CDsoSummaryProperties::get_WordCount(long* plWordCount)
{
    ODS("CDsoSummaryProperties::get_WordCount\n");
	CHECK_NULL_RETURN(plWordCount, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_WORDCOUNT, VT_I4, ((void**)plWordCount));
}

STDMETHODIMP CDsoSummaryProperties::get_CharacterCount(long* plCharacterCount)
{
    ODS("CDsoSummaryProperties::get_CharacterCount\n");
	CHECK_NULL_RETURN(plCharacterCount, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_CHARCOUNT, VT_I4, ((void**)plCharacterCount));
}

STDMETHODIMP CDsoSummaryProperties::get_Thumbnail(VARIANT* pvtThumbnail)
{
    HRESULT hr = S_FALSE;
    CDsoDocProperty* pitem;

    ODS("CDsoSummaryProperties::get_Thumbnail\n");
	CHECK_NULL_RETURN(pvtThumbnail, E_POINTER);
    CHECK_FLAG_RETURN(m_fDeadObj, DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

 // Get thumbnail item from the collection (if it was added).
    pitem = GetPropertyFromList(m_pSummPropList, PIDSI_THUMBNAIL, FALSE);
    if (pitem) hr = pitem->get_Value(pvtThumbnail);
    return hr;
}

STDMETHODIMP CDsoSummaryProperties::get_ApplicationName(BSTR* pbstrAppName)
{
    ODS("CDsoSummaryProperties::get_ApplicationName\n");
	CHECK_NULL_RETURN(pbstrAppName, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_APPNAME, VT_BSTR, ((void**)pbstrAppName));
}

STDMETHODIMP CDsoSummaryProperties::get_DocumentSecurity(long* plDocSecurity)
{
    ODS("CDsoSummaryProperties::get_DocumentSecurity\n");
	CHECK_NULL_RETURN(plDocSecurity, E_POINTER);
	return ReadProperty(m_pSummPropList, PIDSI_DOC_SECURITY, VT_I4, ((void**)plDocSecurity));
}

////////////////////////////////////////////////////////////////////////
// FMTID_DocSummaryInformation Properties...
//  
STDMETHODIMP CDsoSummaryProperties::get_Category(BSTR* pbstrCategory)
{
    ODS("CDsoSummaryProperties::get_Category\n");
	CHECK_NULL_RETURN(pbstrCategory, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_CATEGORY, VT_BSTR, ((void**)pbstrCategory));
}

STDMETHODIMP CDsoSummaryProperties::put_Category(BSTR bstrCategory)
{
    TRACE1("CDsoSummaryProperties::put_Category(%S)\n", bstrCategory);
	return WriteProperty(&m_pDocPropList, PID_CATEGORY, VT_BSTR, ((void*)bstrCategory));
}

STDMETHODIMP CDsoSummaryProperties::get_PresentationFormat(BSTR* pbstrPresFormat)
{
    ODS("CDsoSummaryProperties::get_PresentationFormat\n");
	CHECK_NULL_RETURN(pbstrPresFormat, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_PRESFORMAT, VT_BSTR, ((void**)pbstrPresFormat));
}

STDMETHODIMP CDsoSummaryProperties::get_ByteCount(long* plByteCount)
{
    ODS("CDsoSummaryProperties::get_ByteCount\n");
	CHECK_NULL_RETURN(plByteCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_BYTECOUNT, VT_I4, ((void**)plByteCount));
}

STDMETHODIMP CDsoSummaryProperties::get_LineCount(long* plLineCount)
{
    ODS("CDsoSummaryProperties::get_LineCount\n");
	CHECK_NULL_RETURN(plLineCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_LINECOUNT, VT_I4, ((void**)plLineCount));
}

STDMETHODIMP CDsoSummaryProperties::get_ParagraphCount(long* plParagraphCount)
{
    ODS("CDsoSummaryProperties::get_ParagraphCount\n");
	CHECK_NULL_RETURN(plParagraphCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_PARACOUNT, VT_I4, ((void**)plParagraphCount));
}

STDMETHODIMP CDsoSummaryProperties::get_SlideCount(long* plSlideCount)
{
    ODS("CDsoSummaryProperties::get_SlideCount\n");
	CHECK_NULL_RETURN(plSlideCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_SLIDECOUNT, VT_I4, ((void**)plSlideCount));
}

STDMETHODIMP CDsoSummaryProperties::get_NoteCount(long* plNoteCount)
{
    ODS("CDsoSummaryProperties::get_NoteCount\n");
	CHECK_NULL_RETURN(plNoteCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_NOTECOUNT, VT_I4, ((void**)plNoteCount));
}

STDMETHODIMP CDsoSummaryProperties::get_HiddenSlideCount(long* plHiddenSlideCount)
{
    ODS("CDsoSummaryProperties::get_HiddenSlideCount\n");
	CHECK_NULL_RETURN(plHiddenSlideCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_HIDDENCOUNT, VT_I4, ((void**)plHiddenSlideCount));
}

STDMETHODIMP CDsoSummaryProperties::get_MultimediaClipCount(long* plMultimediaClipCount)
{
    ODS("CDsoSummaryProperties::get_MultimediaClipCount\n");
	CHECK_NULL_RETURN(plMultimediaClipCount, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_MMCLIPCOUNT, VT_I4, ((void**)plMultimediaClipCount));
}

STDMETHODIMP CDsoSummaryProperties::get_Manager(BSTR* pbstrManager)
{
    ODS("CDsoSummaryProperties::get_Manager\n");
	CHECK_NULL_RETURN(pbstrManager, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_MANAGER, VT_BSTR, ((void**)pbstrManager));
}

STDMETHODIMP CDsoSummaryProperties::put_Manager(BSTR bstrManager)
{
    TRACE1("CDsoSummaryProperties::put_Manager(%S)\n", bstrManager);
	return WriteProperty(&m_pDocPropList, PID_MANAGER, VT_BSTR, ((void*)bstrManager));
}

STDMETHODIMP CDsoSummaryProperties::get_Company(BSTR* pbstrCompany)
{
    ODS("CDsoSummaryProperties::get_Company\n");
	CHECK_NULL_RETURN(pbstrCompany, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_COMPANY, VT_BSTR, ((void**)pbstrCompany));
}

STDMETHODIMP CDsoSummaryProperties::put_Company(BSTR bstrCompany)
{
    TRACE1("CDsoSummaryProperties::put_Company(%S)\n", bstrCompany);
	return WriteProperty(&m_pDocPropList, PID_COMPANY, VT_BSTR, ((void*)bstrCompany));
}

STDMETHODIMP CDsoSummaryProperties::get_CharacterCountWithSpaces(long* plCharCountWithSpaces)
{
    ODS("CDsoSummaryProperties::get_CharacterCountWithSpaces\n");
	CHECK_NULL_RETURN(plCharCountWithSpaces, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_CCHWITHSPACES, VT_I4, ((void**)plCharCountWithSpaces));
}

STDMETHODIMP CDsoSummaryProperties::get_SharedDocument(VARIANT_BOOL* pbSharedDocument)
{
    ODS("CDsoSummaryProperties::get_SharedDocument\n");
	CHECK_NULL_RETURN(pbSharedDocument, E_POINTER);
	return ReadProperty(m_pDocPropList, PID_SHAREDDOC, VT_BOOL, ((void**)pbSharedDocument));
}

STDMETHODIMP CDsoSummaryProperties::get_Version(BSTR* pbstrVersion)
{
    ULONG ul = 0;
    ODS("CDsoSummaryProperties::get_Version\n");
	CHECK_NULL_RETURN(pbstrVersion, E_POINTER); *pbstrVersion = NULL;
    if (SUCCEEDED(ReadProperty(m_pDocPropList, PID_VERSION, VT_I4, (void**)&ul)))
    {
        CHAR szVersion[128];
        wsprintf(szVersion, "%d.%d", (LONG)(HIWORD(ul)), (LONG)(LOWORD(ul)));
        *pbstrVersion = DsoConvertToBSTR(szVersion, CP_ACP);
    }
    return S_OK;
}

STDMETHODIMP CDsoSummaryProperties::get_DigitalSignature(VARIANT* pvtDigSig)
{
    HRESULT hr = S_FALSE;
    CDsoDocProperty* pitem;

    ODS("CDsoSummaryProperties::get_DigitalSignature\n");
	CHECK_NULL_RETURN(pvtDigSig, E_POINTER);
    CHECK_FLAG_RETURN(m_fDeadObj, DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

 // Get DigSig data as CF_BLOB...
    pitem = GetPropertyFromList(m_pDocPropList, PID_DIGSIG, FALSE);
    if (pitem) hr = pitem->get_Value(pvtDigSig);
    return hr;
}

////////////////////////////////////////////////////////////////////////
// Internal Functions
//  
////////////////////////////////////////////////////////////////////////
// LoadProperties -- Reads in both property sets and provides link list
//   of the properties for each (we keep separatelists because each may
//   have overlapping PROPIDs and different CodePages).
//  
STDMETHODIMP CDsoSummaryProperties::LoadProperties(IPropertySetStorage* pPropSS, BOOL fIsReadOnly, dsoFileOpenOptions dwFlags)
{
    HRESULT hr = S_OK;;
    IPropertyStorage *pProps;

 // We only do this once (when loading from file)...
	TRACE2("CDsoSummaryProperties::LoadProperties(ReadOnly=%d, Flags=%d)\n", fIsReadOnly, dwFlags);
    ASSERT(m_pSummPropList == NULL); ASSERT(m_pDocPropList == NULL);

 // First, load the FMTID_SummaryInformation properties...
    hr = DsoOpenPropertyStorage(pPropSS, FMTID_SummaryInformation, fIsReadOnly, dwFlags, &pProps);
    if (SUCCEEDED(hr))
    {
		ODS(" -> Loading FMTID_SummaryInformation... \n");

     // Load all the properties into a list set (and save the code page). The list
     // may return NULL if no properties are found, but that is OK. We just return
     // blank values for items as if they were set to zero...
        hr = DsoLoadPropertySetList(pProps, &m_wCodePageSI, &m_pSummPropList);
        pProps->Release();
    }
    else
    {
     // In cases where the propset is not in the file and it is read-only open
     // or a case where DontAutoCreate flag is used, we just treat as read-only
     // with no properties. Otherwise we return error that propset is invalid...
        if (hr == STG_E_FILENOTFOUND) 
        { // We allow partial open if NoAutoCreate is set.
            if ((fIsReadOnly) || (dwFlags & dsoOptionDontAutoCreate))
            {
                fIsReadOnly = TRUE;
                hr = S_FALSE;
            }
            else hr = DSO_E_INVALIDPROPSET;
        }
    }
    RETURN_ON_FAILURE(hr);

 // Second, load the FMTID_DocSummaryInformation properties...
    hr = DsoOpenPropertyStorage(pPropSS, FMTID_DocSummaryInformation, fIsReadOnly, dwFlags, &pProps);
    if (SUCCEEDED(hr))
    {
		ODS(" -> Loading FMTID_DocSummaryInformation... \n");

     // Load all the properties into a list set (and save the code page). The list
     // may return NULL if no properties are found, but that is OK. We just return
     // blank values for items as if they were set to zero...
        hr = DsoLoadPropertySetList(pProps, &m_wCodePageDSI, &m_pDocPropList);
        pProps->Release();
    }
    else
    {
     // In cases where the propset is not in the file and it is read-only open
     // or a case where DontAutoCreate flag is used, we just treat as read-only
     // with no properties. Otherwise we return error that propset is invalid...
        if (hr == STG_E_FILENOTFOUND) 
        { // We allow partial open if NoAutoCreate is set.
            if ((fIsReadOnly) || (dwFlags & dsoOptionDontAutoCreate))
            {
                fIsReadOnly = TRUE;
                hr = S_FALSE;
            }
            else hr = DSO_E_INVALIDPROPSET;
        }
    }
    
 // If all wen well, store the parameters passed for later use...
    if (SUCCEEDED(hr))
    {
        m_pPropSetStg = pPropSS;
        ADDREF_INTERFACE(m_pPropSetStg);
        m_fReadOnly = fIsReadOnly;
        m_dwFlags = dwFlags;
    }

    return hr;
}

////////////////////////////////////////////////////////////////////////
// ReadProperty -- Reads property of standard type and copies to pv...
//  
STDMETHODIMP CDsoSummaryProperties::ReadProperty(CDsoDocProperty* pPropList, PROPID pid, VARTYPE vt, void** ppv)
{
	HRESULT hr = S_FALSE;
    CDsoDocProperty* pitem;
	VARIANT vtTmp;

	TRACE1("CDsoSummaryProperties::ReadProperty(id=%d)\n", pid);
	ASSERT(ppv); *ppv = NULL;

    CHECK_FLAG_RETURN(m_fDeadObj, DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

    pitem = GetPropertyFromList(pPropList, pid, FALSE);
    if (pitem)
    {
	    VariantInit(&vtTmp);
        hr = pitem->get_Value(&vtTmp);
        if (SUCCEEDED(hr))
        {
         // If data returned is not in the expected type, try to convert it...
            if ((vtTmp.vt != vt) && 
                (FAILED(VariantChangeType(&vtTmp, &vtTmp, 0, vt))))
                return S_FALSE; // E_UNEXPECTED; FIX - 2/18/2000 (return S_FALSE same as missing).

         // Return the native data based on the VT type...
            switch (vt)
		    {
		    case VT_BSTR: *((BSTR*)ppv) = SysAllocString(vtTmp.bstrVal); break;
            case VT_I4:   *((LONG*)ppv) = vtTmp.lVal; break;
            case VT_BOOL: *((VARIANT_BOOL*)ppv) = vtTmp.boolVal; break;
		    case VT_DATE: VariantCopy(((VARIANT*)ppv), &vtTmp); break;
		    }
            VariantClear(&vtTmp);
        }
    }

	return hr;
}

////////////////////////////////////////////////////////////////////////
// WriteProperty -- Writes property of standard type (can append the list)...
//  
STDMETHODIMP CDsoSummaryProperties::WriteProperty(CDsoDocProperty** ppPropList, PROPID pid, VARTYPE vt, void* pv)
{
	HRESULT hr = S_FALSE;
    CDsoDocProperty* pitem;
	VARIANT vtItem; 

 // Going to add property to list. Make sure we allow writes, and make sure prop list exists...
	TRACE1("CDsoSummaryProperties::WriteProperty(id=%d)\n", pid);
    CHECK_NULL_RETURN(ppPropList,  E_POINTER);
    CHECK_FLAG_RETURN(m_fDeadObj,  DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));
    CHECK_FLAG_RETURN(m_fReadOnly, DsoReportError(DSO_E_DOCUMENTREADONLY, NULL, m_pDispExcep));

 // We load Variant based on selected VT (only handle values of 32-bit or lower)
 // This would be a problem for generic read/write, but for summary and doc summary
 // properties, these are the only common types this component handles (so we are OK with this).
	vtItem.vt = vt;
    switch (vt)
    {
      case VT_BSTR: vtItem.bstrVal = ((BSTR)pv); break;
      case VT_I4:   vtItem.lVal    = ((LONG)pv); break;
      //case VT_BOOL: vtItem.boolVal = ((VARIANT_BOOL)((LONG)pv)); break;
	  //case VT_DATE: VariantCopy(&vtItem, ((VARIANT*)pv)); break;
      default: return E_INVALIDARG;
    }

 // Find the cached item in the list and update it...
    pitem = GetPropertyFromList(*ppPropList, pid, TRUE);
    if (pitem)
    {
        hr = pitem->put_Value(&vtItem);

     // Special case where we are adding new property to list that had 
     // no properties to start with (NTFS5 files mostly, since normal 
     // Office/OLE files will have at least one summ/docsumm prop...
        if ((*ppPropList == NULL) && SUCCEEDED(hr))
            *ppPropList = pitem;
    }
    else
    {
     // If it is not in the list, nor can be added...
        hr = E_ACCESSDENIED;
    }

	return hr;
}

////////////////////////////////////////////////////////////////////////
// SaveProperties -- Save all changes.
//  
STDMETHODIMP CDsoSummaryProperties::SaveProperties(BOOL fCommitChanges)
{
    HRESULT hr = S_FALSE;
    IPropertyStorage *pProps;
    ULONG cSaved, cTotalItems = 0;

 // We don't need to do save if in read-only mode...
	TRACE1("CDsoSummaryProperties::SaveProperties(Commit=%d)\n", (ULONG)fCommitChanges);
    CHECK_FLAG_RETURN(m_fReadOnly, DSO_E_DOCUMENTREADONLY);

 // Save the Summary properties..
    if (m_pSummPropList)
    {
        hr = DsoOpenPropertyStorage(m_pPropSetStg, FMTID_SummaryInformation, FALSE, 0, &pProps);
        if (SUCCEEDED(hr))
        {
			ODS(" -> Saving FMTID_SummaryInformation... \n");

         // Save all the changed items in the list...
            hr = DsoSavePropertySetList(pProps, m_wCodePageSI, m_pSummPropList, &cSaved);
            
         // Commit the changes to the file...
            if (SUCCEEDED(hr) && (cSaved) && (fCommitChanges))
                hr = pProps->Commit(STGC_DEFAULT);

            cTotalItems = cSaved;
            pProps->Release();
        }
    }

 // Then save doc summary properties...
    if (SUCCEEDED(hr) && (m_pDocPropList))
    {
        hr = DsoOpenPropertyStorage(m_pPropSetStg, FMTID_DocSummaryInformation, FALSE, 0, &pProps);
        if (SUCCEEDED(hr))
        {
			ODS(" -> Saving FMTID_DocSummaryInformation... \n");

         // Save all the changed items in the list...
            hr = DsoSavePropertySetList(pProps, m_wCodePageDSI, m_pDocPropList, &cSaved);
            
         // Commit the changes to the file...
            if (SUCCEEDED(hr) && (cSaved) && (fCommitChanges))
                hr = pProps->Commit(STGC_DEFAULT);

            cTotalItems += cSaved;
            pProps->Release();
        }
    }

    return (FAILED(hr) ? hr : ((cTotalItems) ? S_OK : S_FALSE));
}

////////////////////////////////////////////////////////////////////////
// GetPropertyFromList -- Enumerates a list and finds item with the 
//    matching id. It can also add a new item (if flag set).
//  
STDMETHODIMP_(CDsoDocProperty*) CDsoSummaryProperties::GetPropertyFromList(CDsoDocProperty* plist, PROPID id, BOOL fAppendNew)
{
    CDsoDocProperty* pitem = plist;
    CDsoDocProperty* plast = pitem;

	ODS("CDsoSummaryProperties::FindPropertyInList\n");

 // Loop the list until you find the item...
    while (pitem)
    {
        if (pitem->GetID() == id)
            break;

        plast = pitem;
        pitem = pitem->GetNextProperty();
    }

// FIXED: 3/25/2006 - The code below should have followed same rules as 
// CDsoCustomProperties::GetPropertyFromList, but it didn't in prior versions. 
// That caused some users of dsofile to get Access Denied errors trying to set 
// summary properties on non-OLE files if that file never had any of those properties
// set prior. This is because NTFS propsets may return a set with zero properties in it.
// This was unusual in Win2K where a file without properties would have NULL propset,
// (and we had to add it explicit) but in WinXP the prop set is not NULL but has zero 
// properties. So the "fix" is to allow this property to be added (if caller requests) 
// even if there are no properties in the list to start with. This is more consistent 
// with the same code we used for custom prop set, and works regardless of OS behavior.

// If no match is found, we can add a new item to end of the list...
    if ((pitem == NULL) && (fAppendNew))
    {
     // Create the item...
        pitem = new CDsoDocProperty();
        if (pitem)
        {
            CDsoDocProperty* pchain = NULL;
            VARIANT var; var.vt = VT_EMPTY;

         // If we are adding item to existing list, append the item...
            if (plast) pchain = plast->AppendLink(pitem);

         // Init the property with name passed...
            if (FAILED(pitem->InitProperty(NULL, id, &var, TRUE, pchain)))
            { // If we fail, reverse the append, and kill the new object...
                if (plast) plast->AppendLink(pchain);
                pitem->Release();
                pitem = NULL;
            }
        }
    }

    return pitem;
}

////////////////////////////////////////////////////////////////////////
// FIsDirty -- Checks both lists to see if changes were made.
//  
STDMETHODIMP_(BOOL) CDsoSummaryProperties::FIsDirty()
{
    BOOL fDirty = FALSE;
    CDsoDocProperty* pitem;
    
	ODS("CDsoSummaryProperties::FIsDirty\n");

 // Loop through summary items and see if any have changed...
    pitem = m_pSummPropList;
    while (pitem)
    {
        if (pitem->IsDirty()){ fDirty = TRUE; break; }
        pitem = pitem->GetNextProperty();
    }

 // Look through doc summary items to see if those changed...
    if (!fDirty)
    {
        pitem = m_pDocPropList;
        while (pitem)
        {
            if (pitem->IsDirty()){ fDirty = TRUE; break; }
            pitem = pitem->GetNextProperty();
        }
    }

    return fDirty;
}

////////////////////////////////////////////////////////////////////////
// Disconnect -- Parent is closing, so disconnect object.
//  
STDMETHODIMP_(void) CDsoSummaryProperties::Disconnect()
{
    CDsoDocProperty *pitem, *pnext;
	ODS("CDsoSummaryProperties::Disconnect\n");

 // Loop through both lists and disconnect item (this should free them)...
    pitem = m_pSummPropList; m_pSummPropList = NULL;
    while (pitem)
    {
        pnext = pitem->GetNextProperty();
        pitem->Disconnect(); pitem = pnext;
    }

    pitem = m_pDocPropList; m_pDocPropList = NULL;
    while (pitem)
    {
        pnext = pitem->GetNextProperty();
        pitem->Disconnect(); pitem = pnext;
    }


 // We are now dead ourselves (release prop storage ref)...
    RELEASE_INTERFACE(m_pPropSetStg);
    m_fDeadObj = TRUE;

 // Call release on ourself (this will free the object on last ref)...
    Release();
    return;
}
