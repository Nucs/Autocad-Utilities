/***************************************************************************
 * CUSTPROPS.CPP
 *
 * Implementation for CustomProperties Object
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
// CDsoCustomProperties
//
////////////////////////////////////////////////////////////////////////
// Class Constructor/Destructor
//
CDsoCustomProperties::CDsoCustomProperties()
{
	ODS("CDsoCustomProperties::CDsoCustomProperties()\n");
    m_cRef           = 1; // Automatically has one ref...
    m_ptiDispType    = NULL;
    m_pDispExcep     = NULL;
    m_pPropSetStg    = NULL;
    m_dwFlags        = dsoOptionDefault;
    m_fReadOnly      = FALSE;
    m_fExternal      = FALSE;
    m_fDeadObj       = FALSE;
    m_wCodePage      = CP_ACP;
    m_pCustPropList  = NULL;
    m_pCurrentProp   = NULL;
}

CDsoCustomProperties::~CDsoCustomProperties(void)
{
    ODS("CDsoCustomProperties::~CDsoCustomProperties()\n");
    RELEASE_INTERFACE(m_ptiDispType);
}

////////////////////////////////////////////////////////////////////////
// IUnknown Implementation
//
////////////////////////////////////////////////////////////////////////
// QueryInterface
//
STDMETHODIMP CDsoCustomProperties::QueryInterface(REFIID riid, void** ppv)
{
	HRESULT hr = S_OK;

	ODS("CDsoCustomProperties::QueryInterface\n");
	CHECK_NULL_RETURN(ppv, E_POINTER);
	
	if ((IID_CustomProperties == riid) || (IID_IDispatch == riid) || (IID_IUnknown == riid))
    {
		*ppv = (CustomProperties*)this;
    }
	else if (IID_ISupportErrorInfo == riid)
    {
		*ppv = (ISupportErrorInfo*)this;
    }
	else if (IID_IEnumVARIANT == riid)
    {
		*ppv = (IEnumVARIANT*)this;
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
STDMETHODIMP_(ULONG) CDsoCustomProperties::AddRef(void)
{
	TRACE1("CDsoCustomProperties::AddRef - %d\n", m_cRef+1);
    if (!m_fExternal) { DllAddRef(); m_fExternal = TRUE; }
    return ++m_cRef;
}

////////////////////////////////////////////////////////////////////////
// Release
//
STDMETHODIMP_(ULONG) CDsoCustomProperties::Release(void)
{
	ULONG ul = --m_cRef;
	TRACE1("CDsoCustomProperties::Release - %d\n", ul);

    if (ul == 0)
	{
        ASSERT(m_fDeadObj); // We better be dead before going away!
		ODS("CDsoCustomProperties Final Release\n");
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
STDMETHODIMP CDsoCustomProperties::GetTypeInfoCount(UINT* pctinfo)
{
	ODS("CDsoCustomProperties::GetTypeInfoCount\n");
	if (pctinfo) *pctinfo = 1;
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetTypeInfo
//
STDMETHODIMP CDsoCustomProperties::GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
{
	ODS("CDsoCustomProperties::GetTypeInfo\n");
	CHECK_NULL_RETURN(ppTInfo, E_POINTER);
    *ppTInfo = NULL;

 // We only support default type info...
	CHECK_NULL_RETURN((iTInfo == 0), DISP_E_BADINDEX);

 // Load type info if we have not done so already
    if (NULL == m_ptiDispType)
    {
        HRESULT hr = DsoGetTypeInfo(IID_CustomProperties, &m_ptiDispType);
        RETURN_ON_FAILURE(hr);
    }

    *ppTInfo = m_ptiDispType;
    ADDREF_INTERFACE(((IUnknown*)*ppTInfo));
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetIDsOfNames
//
STDMETHODIMP CDsoCustomProperties::GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoCustomProperties::GetIDsOfNames\n");
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
STDMETHODIMP CDsoCustomProperties::Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoCustomProperties::Invoke\n");
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
// CustomProperties Implementation
//
////////////////////////////////////////////////////////////////////////
// get_Count - Number of items in the list (not including items marked for remove)
//
STDMETHODIMP CDsoCustomProperties::get_Count(long* lCount)
{
    CDsoDocProperty* pitem;
    ULONG ul = 0;

    CHECK_NULL_RETURN(lCount,  E_POINTER);
    CHECK_FLAG_RETURN(m_fDeadObj, DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

    pitem = m_pCustPropList;
    while (pitem)
    { 
        if (!(pitem->IsRemoved())) ++ul; 
        pitem = pitem->GetNextProperty();
    }

    *lCount = ul;
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// Add - Adds the given item to list with BSTR name.  
//
STDMETHODIMP CDsoCustomProperties::Add(BSTR sPropName, VARIANT* Value, CustomProperty** ppDocProperty)
{
    HRESULT hr;
    CDsoDocProperty* pitem;

 // Add an item to the collection. Check the stat of the object and validate
 // the parameters passed. 
	TRACE1("CDsoCustomProperties::Add(%S)\n", sPropName);
    CHECK_NULL_RETURN(ppDocProperty,   E_POINTER); *ppDocProperty = NULL;
    CHECK_NULL_RETURN(sPropName,       E_INVALIDARG);
    CHECK_NULL_RETURN((*sPropName),    E_INVALIDARG);
    CHECK_NULL_RETURN(Value,           E_INVALIDARG);
    CHECK_FLAG_RETURN(m_fDeadObj,      DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

 // First, the caller provided a name and it is not in the list (at least 
 // as an non-removed item -- if it is removed, we can reuse it)...
    pitem = GetPropertyFromList(m_pCustPropList, sPropName, FALSE);
    if ((pitem) && !(pitem->IsRemoved()))
        return DsoReportError(DSO_E_ITEMEXISTS, NULL, m_pDispExcep);

 // Next, find or add a property by the name and place the new data
 // in it. Note that this is basically going through the list again to
 // append the new item to the end of the chain...
    pitem = GetPropertyFromList(m_pCustPropList, sPropName, TRUE);
    if (pitem)
    {
     // Restore the object if deleted once.
        if (pitem->IsRemoved()) pitem->Renew();

     // Set the new value...
        hr = pitem->put_Value(Value);
        if (SUCCEEDED(hr))
        { 
         // Return the automation interface for the caller...
            hr = pitem->QueryInterface(IID_CustomProperty, (void**)ppDocProperty);

         // Special case where we have no custom properties to start with
         // we need to use the first property added as start of the list...
            if ((m_pCustPropList == NULL) && SUCCEEDED(hr))
                m_pCustPropList = pitem;
        } 
        else pitem->Remove(); // If put_Value fails, mark item as removed

    }
    else hr = E_ACCESSDENIED;

    return hr;
}

////////////////////////////////////////////////////////////////////////
// get_Item - Return the spcific item (by name or number index)
//
STDMETHODIMP CDsoCustomProperties::get_Item(VARIANT Index, CustomProperty** ppDocProperty)
{
    HRESULT hr;
    CDsoDocProperty* pitem;
    BSTR bstrName = NULL;

 // Get item from the list (make sure we are not zombie and we have list)...
	ODS("CDsoCustomProperties::get_Item()\n");
    CHECK_NULL_RETURN(ppDocProperty,   E_POINTER); *ppDocProperty = NULL;
    CHECK_FLAG_RETURN(m_fDeadObj,      DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

 // If the index is a string, attempt to locate it by name...
    if ((Index.vt & VT_TYPEMASK) == VT_BSTR)
    {
     // Grab temp bstr -- need to watch for possible indirection...
        if ((Index.vt & VT_BYREF) == VT_BYREF)
            bstrName = *(Index.pbstrVal);
        else
            bstrName = Index.bstrVal;

     // Must have valid name, else return E_INVALIDARG...
        if ((NULL == bstrName) || (0 == SysStringLen(bstrName)))
            return E_INVALIDARG;

     // Find the item...
        pitem = GetPropertyFromList(m_pCustPropList, bstrName, FALSE);
    }
    else
    {
     // If Index is not a string, see if it converts to number index...
        VARIANT vtIdx; vtIdx.vt = VT_EMPTY;
        hr = VariantChangeType(&vtIdx, &Index, 0, VT_I4);
        CHECK_FLAG_RETURN(FAILED(hr), E_INVALIDARG);

     // Find the item by that number...
        pitem = GetPropertyByCount(m_pCustPropList, (ULONG)(vtIdx.lVal));
    }

 // If we don't have the item, or it was deleted, return error...
    if ((pitem == NULL) || (pitem->IsRemoved()))
    {
     // ADDED 2/10/2001 -- BY REQUEST
     // Return a more helpful string error if the named item isn't in the collection.
        LPWSTR pwszError = NULL;
        WCHAR wszError[1024];
        if ((bstrName) && (v_fRunningOnNT))
        {
            wsprintfW(wszError, L"The property \"%s\" does not exist in the collection.", bstrName);
            pwszError = wszError;
        }
        return DsoReportError(DSO_E_INVALIDINDEX, pwszError, m_pDispExcep);
    }

    hr = pitem->QueryInterface(IID_CustomProperty, (void**)ppDocProperty);
    return hr;
}

////////////////////////////////////////////////////////////////////////
// get__NewEnum - Used by VB For Each
//
STDMETHODIMP CDsoCustomProperties::get__NewEnum(IUnknown** ppunk)
{ 
    CHECK_FLAG_RETURN(m_fDeadObj, DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));
    m_pCurrentProp = m_pCustPropList; // Reset the enum list for new "for each"
    return QueryInterface(IID_IEnumVARIANT, (void**)ppunk);
}

////////////////////////////////////////////////////////////////////////
// IEnumVARIANT Implementation
//
////////////////////////////////////////////////////////////////////////
// Next
//
STDMETHODIMP CDsoCustomProperties::Next(ULONG cElements, VARIANT* pvar, ULONG* pcElementFetched)
{
    CDsoDocProperty* pitem = m_pCurrentProp;
    IDispatch *pdisp;
    UINT i = 0;

 // Get item from the list (make sure we are not zombie and we have list)...
	ODS("CDsoCustomProperties::Next()\n");
    CHECK_NULL_RETURN(pvar, E_POINTER);
    CHECK_NULL_RETURN(cElements,  E_INVALIDARG);
    CHECK_FLAG_RETURN(m_fDeadObj, DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

 // Loop through items. Only fetch non-removed items. Note we return
 // with ref on interface pointer so QI is not released here.
    while ((pitem) && (cElements))
    {
        if (!(pitem->IsRemoved()))
        {
            pitem->QueryInterface(IID_IDispatch, (void**)&pdisp);
            pvar[i].vt = VT_DISPATCH;
            pvar[i].pdispVal = pdisp;
            ++i; --cElements;
        }
        pitem = pitem->GetNextProperty();
    }

 // Save last position to start from this point again, and return
 // the fetch count (normally this is 1 since VB asks one at a time)...
    m_pCurrentProp = pitem;
    if (pcElementFetched) *pcElementFetched = i;
    return ((i) ? S_OK : S_FALSE);
}

////////////////////////////////////////////////////////////////////////
// Skip
//
STDMETHODIMP CDsoCustomProperties::Skip(ULONG cElements)
{ // Move pointer ahead by cElements
    m_pCurrentProp = GetPropertyByCount(m_pCurrentProp, cElements);
    return ((m_pCurrentProp) ? S_OK : S_FALSE);
}

////////////////////////////////////////////////////////////////////////
// Reset
//
STDMETHODIMP CDsoCustomProperties::Reset()
{ // Reset current prop to start of the chain...
    m_pCurrentProp = m_pCustPropList;
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// Clone
//
STDMETHODIMP CDsoCustomProperties::Clone(IEnumVARIANT** ppenum)
{ if (ppenum) *ppenum = NULL; return E_NOTIMPL; }


////////////////////////////////////////////////////////////////////////
// Internal Functions
//
////////////////////////////////////////////////////////////////////////
// LoadProperties
//
STDMETHODIMP CDsoCustomProperties::LoadProperties(IPropertySetStorage* pPropSS, BOOL fIsReadOnly, dsoFileOpenOptions dwFlags)
{
    HRESULT hr;
    IPropertyStorage *pProps;

 // We only do this once (when loading from file)...
	TRACE2("CDsoCustomProperties::LoadProperties(ReadOnly=%d, Flags=%d)\n", fIsReadOnly, dwFlags);
    ASSERT(m_pCustPropList == NULL);

 // First, load the FMTID_UserDefinedProperties properties...
    hr = DsoOpenPropertyStorage(pPropSS, FMTID_UserDefinedProperties, fIsReadOnly, dwFlags, &pProps);
    if (SUCCEEDED(hr))
    {
		ODS(" -> Loading FMTID_UserDefinedProperties... \n");

     // Load all the properties into a list set (and save the code page). The list
     // may return NULL if no properties are found, but user can add them elsewhere...
        hr = DsoLoadPropertySetList(pProps, &m_wCodePage, &m_pCustPropList);
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

 // If all succeeded, save parameters passed for future use...
    if (SUCCEEDED(hr))
    {
        m_pPropSetStg = pPropSS;
        ADDREF_INTERFACE(m_pPropSetStg);
        m_dwFlags = dwFlags;
        m_fReadOnly = fIsReadOnly;
    }

    return hr;
}

////////////////////////////////////////////////////////////////////////
// GetPropertyFromList  -- Name lookup
//
STDMETHODIMP_(CDsoDocProperty*) CDsoCustomProperties::GetPropertyFromList(CDsoDocProperty* plist, LPCWSTR pwszName, BOOL fAppendNew)
{
    CDsoDocProperty* pitem = plist;
    CDsoDocProperty* plast = pitem;
    BSTR bstrName = NULL;

 // Scan all the items in the list and match by name...
    while (pitem)
    {
        pitem->get_Name(&bstrName);
        if (bstrName)
        {
            if (DsoCompareStrings(bstrName, pwszName) == CSTR_EQUAL)
            {
                SysFreeString(bstrName);
                break;
            }
            SysFreeString(bstrName);
        }

        plast = pitem;
        pitem = pitem->GetNextProperty();
    }

// If no match is found, we may want to add a new item to end of the list...
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
            if (FAILED(pitem->InitProperty((LPWSTR)pwszName, 0, &var, TRUE, pchain)))
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
// GetPropertyByCount  -- Count lookup (no append option)
//
STDMETHODIMP_(CDsoDocProperty*) CDsoCustomProperties::GetPropertyByCount(CDsoDocProperty* plist, ULONG cItem)
{
    CDsoDocProperty* pitem = plist;

    while (pitem)
    {
        if (!(pitem->IsRemoved()))
        {
            if (cItem == 0)
                break;
            --cItem;
        }
        pitem = pitem->GetNextProperty();
    }

    return pitem;
}

////////////////////////////////////////////////////////////////////////
// SaveProperties 
//
STDMETHODIMP CDsoCustomProperties::SaveProperties(BOOL fCommitChanges)
{
    HRESULT hr = S_FALSE;
    IPropertyStorage *pProps;
    ULONG cSavedItems = 0;

 // We don't need to do save if in read-only mode...
	TRACE1("CDsoCustomProperties::SaveProperties(Commit=%d)\n", (ULONG)fCommitChanges);
    CHECK_FLAG_RETURN(m_fReadOnly, DSO_E_DOCUMENTREADONLY);

 // Save the items in the custom property list...
    if (m_pCustPropList)
    {
        hr = DsoOpenPropertyStorage(m_pPropSetStg, FMTID_UserDefinedProperties, FALSE, 0, &pProps);
        if (SUCCEEDED(hr))
        {
			ODS(" -> Saving FMTID_UserDefinedProperties... \n");

         // Save the items that have changed...
            hr = DsoSavePropertySetList(pProps, m_wCodePage, m_pCustPropList, &cSavedItems);

         // Commit the changes to the file...
            if (SUCCEEDED(hr) && (cSavedItems) && (fCommitChanges))
                hr = pProps->Commit(STGC_DEFAULT);

            pProps->Release();
        }
    }

    return (FAILED(hr) ? hr : ((cSavedItems) ? S_OK : S_FALSE));
}

////////////////////////////////////////////////////////////////////////
// FIsDirty 
//
STDMETHODIMP_(BOOL) CDsoCustomProperties::FIsDirty()
{
    BOOL fDirty = FALSE;
    CDsoDocProperty* pitem;
    
	ODS("CDsoCustomProperties::FIsDirty\n");

 // Loop through summary items and see if any have changed...
    pitem = m_pCustPropList;
    while (pitem)
    {
        if (pitem->IsDirty() || pitem->IsRemoved())
            { fDirty = TRUE; break; }
        pitem = pitem->GetNextProperty();
    }

    return fDirty;
}

////////////////////////////////////////////////////////////////////////
// Disconnect 
//
STDMETHODIMP_(void) CDsoCustomProperties::Disconnect()
{
    CDsoDocProperty *pitem, *pnext;
	ODS("CDsoCustomProperties::Disconnect\n");

 // Loop through both lists and disconnect item (this should free them)...
    pitem = m_pCustPropList; m_pCustPropList = NULL;
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

