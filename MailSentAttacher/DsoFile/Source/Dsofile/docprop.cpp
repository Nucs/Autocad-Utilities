/***************************************************************************
 * DOCPROP.CPP
 *
 * Implementation Code for Main Server Object (CDsoDocumentProperties)
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
#include <olectl.h>    //needed for OleCreatePictureIndirect

////////////////////////////////////////////////////////////////////////
// CDsoDocumentProperties
//
////////////////////////////////////////////////////////////////////////
// Class Constructor/Destructor
//
CDsoDocumentProperties::CDsoDocumentProperties()
{
	ODS("CDsoDocumentProperties::CDsoDocumentProperties()\n");
    m_cRef           = 0;
    m_ptiDispType    = NULL;
    m_pDispExcep     = NULL;
	m_bstrFileName   = NULL;
	m_cFilePartIdx   = 0;
    m_pStorage       = NULL;
    m_pPropSetStg    = NULL;
    m_dwFlags        = dsoOptionDefault;
	m_fReadOnly		 = FALSE;
    m_wCodePage      = 0;
    m_pSummProps     = NULL;
    m_pCustomProps   = NULL;
	m_pPrstFile      = NULL;
	m_dwAttributes   = 0;
	m_fResetAttrib   = FALSE;
}

CDsoDocumentProperties::~CDsoDocumentProperties(void)
{
    ODS("CDsoDocumentProperties::~CDsoDocumentProperties()\n");

    ASSERT(m_pPropSetStg == NULL); // We should be closed before delete!
    ASSERT(m_pStorage == NULL);    // We should be closed before delete!
    ASSERT(m_pPrstFile == NULL);   // We should be closed before delete!

    if ((m_pPropSetStg) || (m_pStorage) || (m_pPrstFile))
		Close(VARIANT_FALSE); // Force close now if not free!

    RELEASE_INTERFACE(m_ptiDispType);
}

////////////////////////////////////////////////////////////////////////
// IUnknown Implementation
//
////////////////////////////////////////////////////////////////////////
// QueryInterface
//
STDMETHODIMP CDsoDocumentProperties::QueryInterface(REFIID riid, void** ppv)
{
	HRESULT hr = S_OK;

	ODS("CDsoDocumentProperties::QueryInterface\n");
	CHECK_NULL_RETURN(ppv, E_POINTER);
	
	if ((IID__OleDocumentProperties == riid) || 
        (IID_IDispatch == riid) || (IID_IUnknown == riid))
    {
		*ppv = (_OleDocumentProperties*)this;
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
STDMETHODIMP_(ULONG) CDsoDocumentProperties::AddRef(void)
{
	TRACE1("CDsoDocumentProperties::AddRef - %d\n", m_cRef+1);
    return ++m_cRef;
}

////////////////////////////////////////////////////////////////////////
// Release
//
STDMETHODIMP_(ULONG) CDsoDocumentProperties::Release(void)
{
	ULONG ul = --m_cRef;
	TRACE1("CDsoDocumentProperties::Release - %d\n", ul);

    if (ul == 0)
	{
		ODS("CDsoDocumentProperties Final Release\n");
        if (m_pStorage) Close(VARIANT_FALSE);
        DllRelease(); delete this;
	}
    return ul;
}

////////////////////////////////////////////////////////////////////////
// IDispatch Implementation
//
////////////////////////////////////////////////////////////////////////
// GetTypeInfoCount
//
STDMETHODIMP CDsoDocumentProperties::GetTypeInfoCount(UINT* pctinfo)
{
	ODS("CDsoDocumentProperties::GetTypeInfoCount\n");
	if (pctinfo) *pctinfo = 1;
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetTypeInfo
//
STDMETHODIMP CDsoDocumentProperties::GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
{
	ODS("CDsoDocumentProperties::GetTypeInfo\n");
	CHECK_NULL_RETURN(ppTInfo, E_POINTER);
    *ppTInfo = NULL;

 // We only support default type info...
	CHECK_NULL_RETURN((iTInfo == 0), DISP_E_BADINDEX);

 // Load type info if we have not done so already
    if (NULL == m_ptiDispType)
    {
        HRESULT hr = DsoGetTypeInfo(IID__OleDocumentProperties, &m_ptiDispType);
        RETURN_ON_FAILURE(hr);
    }

    *ppTInfo = m_ptiDispType;
    ADDREF_INTERFACE(((IUnknown*)*ppTInfo));
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetIDsOfNames
//
STDMETHODIMP CDsoDocumentProperties::GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoDocumentProperties::GetIDsOfNames\n");
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
STDMETHODIMP CDsoDocumentProperties::Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoDocumentProperties::Invoke\n");
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
// _OleDocumentProperties Implementation
//
////////////////////////////////////////////////////////////////////////
// Open  -- Takes a full or relative file name and loads the property
//   set for the document. Handles both OLE and NTFS5 property sets.
//
STDMETHODIMP CDsoDocumentProperties::Open(BSTR sFileName, VARIANT_BOOL ReadOnly, dsoFileOpenOptions Options)
{
    HRESULT hr;
    DWORD dwOpenMode;
    WCHAR wszFullName[MAX_PATH];
    ULONG ulIdx;

 // Open method called. Ensure we don't have file already open...
	ODS("CDsoDocumentProperties::Open\n");
    ASSERT(m_pStorage == NULL); // We should only load one at a time per object!
    CHECK_NULL_RETURN((m_pStorage == NULL), DsoReportError(DSO_E_DOCUMENTOPENED, NULL, m_pDispExcep));

 // Validate the name passed and resolve to full path (if relative)...
    CHECK_NULL_RETURN(sFileName, E_INVALIDARG);
    if (!FFindQualifiedFileName(sFileName, wszFullName, &ulIdx))
        return DsoReportError(STG_E_INVALIDNAME, NULL, m_pDispExcep);

 // Check for possible offline file that cannot be open without pulling off backup store.
 // If file is offline we will just fail with custom error in the no props case.
	m_dwAttributes = DsoGetFileAttrib(wszFullName);
	if ((m_dwAttributes & FILE_ATTRIBUTE_OFFLINE) == FILE_ATTRIBUTE_OFFLINE)
		return DsoReportError(DSO_E_NODOCUMENTPROPS, L"File is offline and cannot be read.", m_pDispExcep);

 // Save file name and path index from SearchFile API...
    m_bstrFileName = SysAllocString(wszFullName);
    m_cFilePartIdx = ulIdx;
    if ((m_cFilePartIdx < 1) || (m_cFilePartIdx > SysStringLen(m_bstrFileName))) 
		m_cFilePartIdx = 0;

 // Set open mode flags based on ReadOnly flag (the exclusive access is required for
 // the IPropertySetStorage interface -- which sucks, but we can work around for OLE files)...
    m_fReadOnly = (ReadOnly != VARIANT_FALSE);
    m_dwFlags = Options;
    dwOpenMode = ((m_fReadOnly) ? (STGM_READ | STGM_SHARE_EXCLUSIVE) : (STGM_READWRITE | STGM_SHARE_EXCLUSIVE));

 // Check if file is marked read-only. If caller wants to modify it, let's temporarily remove
 // the attribute so we can set the changes. We will then restore the attributes on close.
	if ((!m_fReadOnly) && (m_dwAttributes & FILE_ATTRIBUTE_READONLY))
	{
		m_fResetAttrib = DsoSetFileAttrib(m_bstrFileName, (m_dwAttributes & ~FILE_ATTRIBUTE_READONLY));
	}

 // If the file is an OLE Storage DocFile...
    if (StgIsStorageFile(m_bstrFileName) == S_OK)
    {
     // Get the data from IStorage...
	    hr = StgOpenStorage(m_bstrFileName, NULL, dwOpenMode, NULL, 0, &m_pStorage);

     // If we failed to gain write access, try to just read access if caller allows
	 // it. This function will open the OLE file in transacted read mode, which
	 // covers cases where the file is in use or is on a read-only share. We can't
	 // save after the open so we force the read-only flag on...
        if (((hr == STG_E_ACCESSDENIED) || (hr == STG_E_SHAREVIOLATION)) && 
            (m_dwFlags & dsoOptionOpenReadOnlyIfNoWriteAccess))
        {
            m_fReadOnly = TRUE;
	        hr = StgOpenStorage(m_bstrFileName, NULL, 
				(STGM_READ | STGM_TRANSACTED | STGM_SHARE_DENY_NONE), NULL, 0, &m_pStorage);
        }
        
	 // If we are lucky, we have a storage to read from, so ask OLE to open the 
	 // associated property set for the file and return the IPSS iface...
	    if (SUCCEEDED(hr))
        {
            hr = m_pStorage->QueryInterface(IID_IPropertySetStorage, (void**)&m_pPropSetStg);
        }
    }
    else if ((m_dwFlags & dsoOptionOnlyOpenOLEFiles) != dsoOptionOnlyOpenOLEFiles)
    {
	 // If caller would like non-OLE property sets, we can try to provide them. There
	 // are two types: (1) explicit metadata handlers registered by file type; or (2) 
	 // NTFS 5.0+ property stream data in NTFS file header (which is actually saved as
	 // alternate stream). The former is custom provider to save data inside the file 
	 // without using OLE. The later is available for any file on NTFS5 disk.
		CLSID clsidMetaHandler = {0};
		IPersistFile *prtsf = NULL;

		if (DsoGetMetaHandler(m_bstrFileName, &clsidMetaHandler) == S_OK)
		{
		 // Create instance of the Metadata Handler object...
			hr = CoCreateInstance(clsidMetaHandler, NULL, CLSCTX_INPROC, IID_IPersistFile, (void**)&prtsf);
			if (SUCCEEDED(hr))
			{
			 // Ask it to load the file for parsing...
				hr = prtsf->Load(m_bstrFileName, dwOpenMode);
				if (SUCCEEDED(hr))
				{
				 // If it succeeded, ask for the property set storage...
					hr = prtsf->QueryInterface(IID_IPropertySetStorage, (void**)&m_pPropSetStg);
					if (SUCCEEDED(hr)){ASSIGN_INTERFACE(m_pPrstFile, prtsf);}
				}
				prtsf->Release();	
			}
			else hr = DSO_E_NODOCUMENTPROPS; // bad news, unable to load handler.
		}
		else if (v_pfnStgOpenStorageEx)
		{
		 // On Win2K+ we can try and open plain files on NTFS 5.0 drive and get 
		 // the NTFS version of OLE properties (saved in alt stream)...
			hr = (v_pfnStgOpenStorageEx)(m_bstrFileName, dwOpenMode, STGFMT_FILE, 0, NULL, 0, 
					IID_IPropertySetStorage, (void**)&m_pPropSetStg);

		 // If we failed to gain write access, try to just read access if caller
		 // wants us to. This only works for access block, not share violations...
		   if ((hr == STG_E_ACCESSDENIED) && (!m_fReadOnly) && 
				(m_dwFlags & dsoOptionOpenReadOnlyIfNoWriteAccess))
			{
				m_fReadOnly = TRUE;
				hr = (v_pfnStgOpenStorageEx)(m_bstrFileName, (STGM_READ | STGM_SHARE_EXCLUSIVE), STGFMT_FILE,
					0, NULL, 0, IID_IPropertySetStorage, (void**)&m_pPropSetStg);
			}
		}
		else
		{ // If we land here, the file is not OLE and not on NTFS5 drive...
			hr = DSO_E_NODOCUMENTPROPS;
		}
    }
    else
    {  // If we land here, the file is not OLE and caller asked for OLE only...
        hr = DSO_E_NODOCUMENTPROPS; 
    }

    if (FAILED(hr))
    {
        DsoReportError(hr, NULL, m_pDispExcep);
        Close(VARIANT_FALSE); // Force a cleanup on error...
    }

    return hr;
}

////////////////////////////////////////////////////////////////////////
// Close  --  Close the open document (optional save before close)
//
STDMETHODIMP CDsoDocumentProperties::Close(VARIANT_BOOL SaveBeforeClose)
{
	ODS("CDsoDocumentProperties::Close\n");

 // If caller requests full save on close, try it. Note that this is the
 // only place where Close will return an error (and NOT close)...
    if (SaveBeforeClose != VARIANT_FALSE)
    {
        HRESULT hr = Save();
        RETURN_ON_FAILURE(hr);
    }

 // The rest is just cleanup to restore us back to state where
 // we can be called again. The Zombie call disconnects sub objects
 // and should free them if caller has also released them...
    ZOMBIE_OBJECT(m_pSummProps);
    ZOMBIE_OBJECT(m_pCustomProps);
    
    RELEASE_INTERFACE(m_pPropSetStg);
    RELEASE_INTERFACE(m_pStorage);
    RELEASE_INTERFACE(m_pPrstFile);

	if (m_fResetAttrib)
		DsoSetFileAttrib(m_bstrFileName, m_dwAttributes);

    FREE_BSTR(m_bstrFileName);

    m_cFilePartIdx = 0;
    m_dwFlags      = dsoOptionDefault;
    m_fReadOnly    = FALSE;
	m_dwAttributes = 0;
	m_fResetAttrib = FALSE;

    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_IsReadOnly - Returns User-Friendly Name for File Type
//
STDMETHODIMP CDsoDocumentProperties::get_IsReadOnly(VARIANT_BOOL* pbReadOnly)
{
	ODS("CDsoDocumentProperties::get_IsReadOnly\n");
	CHECK_NULL_RETURN(pbReadOnly,  E_POINTER); 
    *pbReadOnly = ((m_fReadOnly) ? VARIANT_TRUE : VARIANT_FALSE);
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_IsDirty  -- Have any changes been made to the properties?
//
STDMETHODIMP CDsoDocumentProperties::get_IsDirty(VARIANT_BOOL* pbDirty)
{
    BOOL fDirty = FALSE;
 	ODS("CDsoDocumentProperties::get_IsDirty\n");

 // Check the status of summary properties...
    if ((m_pSummProps) && (m_pSummProps->FIsDirty()))
        fDirty = TRUE;

 // Check the status of custom properties...
    if ((!fDirty) && (m_pCustomProps) && (m_pCustomProps->FIsDirty()))
        fDirty = TRUE;

    if (pbDirty) // Return status to caller...
        *pbDirty = (VARIANT_BOOL)((fDirty) ? VARIANT_TRUE : VARIANT_FALSE);
 
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// Save  --  Will save the changes made back to the document.
//
STDMETHODIMP CDsoDocumentProperties::Save()
{
    HRESULT hr = S_FALSE;
    BOOL fSaveMade = FALSE;

 	ODS("CDsoDocumentProperties::Save\n");
    CHECK_FLAG_RETURN(m_fReadOnly, DsoReportError(DSO_E_DOCUMENTREADONLY, NULL, m_pDispExcep));

 // Ask SummaryProperties to save its changes...
    if (m_pSummProps)
    {
        hr = m_pSummProps->SaveProperties(TRUE);
        if (FAILED(hr)) return DsoReportError(hr, NULL, m_pDispExcep);
        fSaveMade = (hr == S_OK);
    }

 // Ask CustomProperties to save its changes...
    if (m_pCustomProps)
    {
        hr = m_pCustomProps->SaveProperties(TRUE);
        if (FAILED(hr)) return DsoReportError(hr, NULL, m_pDispExcep);
        fSaveMade = ((fSaveMade) || (hr == S_OK));
    }

 // If save was made, commit the root storage before return...
    if (fSaveMade)
	{
		if (m_pStorage)
		{
			hr = m_pStorage->Commit(STGC_DEFAULT);
		}
		else if (m_pPrstFile)
		{
			hr = m_pPrstFile->Save(NULL, FALSE);
			if (SUCCEEDED(hr)) m_pPrstFile->SaveCompleted(NULL);
		}
	}

    return hr;
}

////////////////////////////////////////////////////////////////////////
// get_SummaryProperties - Returns SummaryProperties object
//
STDMETHODIMP CDsoDocumentProperties::get_SummaryProperties(SummaryProperties** ppSummaryProperties)
{
    HRESULT hr;

 	ODS("CDsoDocumentProperties::get_SummaryProperties\n");
    CHECK_NULL_RETURN(ppSummaryProperties,  E_POINTER);
    *ppSummaryProperties = NULL;

    if (m_pSummProps == NULL)
    {
        m_pSummProps = new CDsoSummaryProperties();
        if (m_pSummProps)
            { hr = m_pSummProps->LoadProperties(m_pPropSetStg, m_fReadOnly, m_dwFlags); }
        else hr = E_OUTOFMEMORY;

        if (FAILED(hr))
        {
            ZOMBIE_OBJECT(m_pSummProps);
            return DsoReportError(hr, NULL, m_pDispExcep);
        }
    }

    hr = m_pSummProps->QueryInterface(IID_SummaryProperties, (void**)ppSummaryProperties);
    return hr;
}

////////////////////////////////////////////////////////////////////////
// get_SummaryProperties - Returns CustomProperties object
//
STDMETHODIMP CDsoDocumentProperties::get_CustomProperties(CustomProperties** ppCustomProperties)
{
    HRESULT hr;

 	ODS("CDsoDocumentProperties::get_CustomProperties\n");
    CHECK_NULL_RETURN(ppCustomProperties,  E_POINTER);
    *ppCustomProperties = NULL;

    if (m_pCustomProps == NULL)
    {
        m_pCustomProps = new CDsoCustomProperties();
        if (m_pCustomProps)
            { hr = m_pCustomProps->LoadProperties(m_pPropSetStg, m_fReadOnly, m_dwFlags); }
        else hr = E_OUTOFMEMORY;

        if (FAILED(hr))
        {
            ZOMBIE_OBJECT(m_pCustomProps);
            return DsoReportError(hr, NULL, m_pDispExcep);
        }
    }

    hr = m_pCustomProps->QueryInterface(IID_CustomProperties, (void**)ppCustomProperties);
    return hr;
}

////////////////////////////////////////////////////////////////////////
// get_Icon - Returns OLE StdPicture object with associated icon 
//
STDMETHODIMP CDsoDocumentProperties::get_Icon(IDispatch** ppicIcon)
{
	HICON hIco;

	ODS("CDsoDocumentProperties::get_Icon\n");
	CHECK_NULL_RETURN(ppicIcon,  E_POINTER); *ppicIcon = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));

    if ((m_bstrFileName) && FGetIconForFile(m_bstrFileName, &hIco))
    {
		PICTDESC  icoDesc;
		icoDesc.cbSizeofstruct = sizeof(PICTDESC);
		icoDesc.picType = PICTYPE_ICON;
		icoDesc.icon.hicon = hIco;
		return OleCreatePictureIndirect(&icoDesc, IID_IDispatch, TRUE, (void**)ppicIcon);
    }
    return S_FALSE;
}

////////////////////////////////////////////////////////////////////////
// get_Name - Returns the name of the file (no path)
//
STDMETHODIMP CDsoDocumentProperties::get_Name(BSTR* pbstrName)
{
	ODS("CDsoDocumentProperties::get_Name\n");
	CHECK_NULL_RETURN(pbstrName,  E_POINTER); *pbstrName = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));

	if (m_bstrFileName != NULL && m_cFilePartIdx > 0)
		*pbstrName = SysAllocString((LPOLESTR)&(m_bstrFileName[m_cFilePartIdx]));

	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_Name - Returns the path to the file (no name)
//
STDMETHODIMP CDsoDocumentProperties::get_Path(BSTR* pbstrPath)
{
	ODS("CDsoDocumentProperties::get_Path\n");
	CHECK_NULL_RETURN(pbstrPath,  E_POINTER); *pbstrPath = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));

	if (m_bstrFileName != NULL && m_cFilePartIdx > 0)
	    *pbstrPath = SysAllocStringLen(m_bstrFileName, m_cFilePartIdx);
	
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_IsOleFile - Returns True if file is OLE DocFile
//
STDMETHODIMP CDsoDocumentProperties::get_IsOleFile(VARIANT_BOOL* pIsOleFile)
{
	ODS("CDsoDocumentProperties::get_IsOleFile\n");
	CHECK_NULL_RETURN(pIsOleFile,  E_POINTER);
    *pIsOleFile = ((m_pStorage) ? VARIANT_TRUE : VARIANT_FALSE);
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_Name - Returns CLSID of OLE DocFile 
//
STDMETHODIMP CDsoDocumentProperties::get_CLSID(BSTR* pbstrCLSID)
{
    HRESULT hr;
	STATSTG stat;
	LPOLESTR pwszCLSID = NULL;

	ODS("CDsoDocumentProperties::get_CLSID\n");
	CHECK_NULL_RETURN(pbstrCLSID,  E_POINTER); *pbstrCLSID = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));
    CHECK_NULL_RETURN(m_pStorage, DsoReportError(DSO_E_MUSTHAVESTORAGE, NULL, m_pDispExcep));

    memset(&stat, 0, sizeof(stat));
    hr = m_pStorage->Stat(&stat, STATFLAG_NONAME);
    RETURN_ON_FAILURE(hr);

	hr = StringFromCLSID(stat.clsid, &pwszCLSID);
    if (SUCCEEDED(hr)) *pbstrCLSID = SysAllocString(pwszCLSID);

    FREE_COTASKMEM(pwszCLSID);
	return hr;
}

////////////////////////////////////////////////////////////////////////
// get_ProgID - Returns ProgID of OLE DocFile 
//
STDMETHODIMP CDsoDocumentProperties::get_ProgID(BSTR* pbstrProgID)
{
    HRESULT hr;
	STATSTG stat;
	LPOLESTR pwszProgID = NULL;

	ODS("CDsoDocumentProperties::get_ProgID\n");
	CHECK_NULL_RETURN(pbstrProgID,  E_POINTER); *pbstrProgID = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));
    CHECK_NULL_RETURN(m_pStorage, DsoReportError(DSO_E_MUSTHAVESTORAGE, NULL, m_pDispExcep));

    memset(&stat, 0, sizeof(stat));
    hr = m_pStorage->Stat(&stat, STATFLAG_NONAME);
    RETURN_ON_FAILURE(hr);

	hr = ProgIDFromCLSID(stat.clsid, &pwszProgID);
	if (SUCCEEDED(hr)) *pbstrProgID = SysAllocString(pwszProgID);

    FREE_COTASKMEM(pwszProgID);
	return hr;
}

////////////////////////////////////////////////////////////////////////
// get_OleDocumentFormat - Returns ClipFormat of OLE DocFile 
//
STDMETHODIMP CDsoDocumentProperties::get_OleDocumentFormat(BSTR* pbstrFormat)
{
    HRESULT hr = S_FALSE;
    CLIPFORMAT cf;

	ODS("CDsoDocumentProperties::get_OleDocumentFormat\n");
	CHECK_NULL_RETURN(pbstrFormat,  E_POINTER); *pbstrFormat = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));
    CHECK_NULL_RETURN(m_pStorage, DsoReportError(DSO_E_MUSTHAVESTORAGE, NULL, m_pDispExcep));

    if (SUCCEEDED(ReadFmtUserTypeStg(m_pStorage, &cf, NULL)) == TRUE)
    {
        int i;
        CHAR szName[MAX_PATH] = {0};

        if ((i = GetClipboardFormatName(cf, szName, MAX_PATH)) > 0)
        {
            szName[i] = '\0';
        }
        else
        {
            wsprintf(szName, "ClipFormat 0x%X (%d)", cf, cf);
        }
        *pbstrFormat = DsoConvertToBSTR(szName, CP_ACP);
        hr = ((*pbstrFormat) ? S_OK : E_OUTOFMEMORY);
    }

    return hr;
}

////////////////////////////////////////////////////////////////////////
// get_OleDocumentType - Returns User-Friendly Name for File Type
//
STDMETHODIMP CDsoDocumentProperties::get_OleDocumentType(BSTR* pbstrType)
{
    HRESULT hr = S_FALSE;;
    LPWSTR lpolestr = NULL;

	ODS("CDsoDocumentProperties::get_OleDocumentType\n");
	CHECK_NULL_RETURN(pbstrType,  E_POINTER); *pbstrType = NULL;
    CHECK_NULL_RETURN(m_pPropSetStg, DsoReportError(DSO_E_DOCUMENTNOTOPEN, NULL, m_pDispExcep));
    CHECK_NULL_RETURN(m_pStorage, DsoReportError(DSO_E_MUSTHAVESTORAGE, NULL, m_pDispExcep));

    if (SUCCEEDED(ReadFmtUserTypeStg(m_pStorage, NULL, &lpolestr)) == TRUE)
    {
        *pbstrType = SysAllocString(lpolestr);
        hr = ((*pbstrType) ? S_OK : E_OUTOFMEMORY);
        FREE_COTASKMEM(lpolestr);
    }

    return hr;
}
