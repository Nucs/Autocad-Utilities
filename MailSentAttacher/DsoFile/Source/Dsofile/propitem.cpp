/***************************************************************************
 * PROPITEM.CPP
 *
 * Implementation Code for Single Property Class (which doubles 
 *  as CustomProperty object) for both summary and custom lists.
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
// CDsoDocProperty
//
////////////////////////////////////////////////////////////////////////
// Class Constructor/Destructor
//
CDsoDocProperty::CDsoDocProperty()
{
	ODS("CDsoDocProperty::CDsoDocProperty()\n");
    m_cRef           = 1; // Automatically has one ref...
    m_ptiDispType    = NULL;
    m_pDispExcep     = NULL;
	m_bstrName       = NULL;
	m_ulPropID       = 0;
    m_vValue.vt      = VT_EMPTY;
	m_fModified		 = FALSE;
    m_fExternal      = FALSE;
    m_fDeadObj       = FALSE;
    m_fNewItem       = FALSE;
    m_fRemovedItem   = FALSE;
    m_pNextItem      = NULL;
}

CDsoDocProperty::~CDsoDocProperty(void)
{
    ODS("CDsoDocProperty::~CDsoDocProperty()\n");
    RELEASE_INTERFACE(m_ptiDispType);
    FREE_BSTR(m_bstrName); VariantClear(&m_vValue);
}

////////////////////////////////////////////////////////////////////////
// IUnknown Implementation
//
////////////////////////////////////////////////////////////////////////
// QueryInterface
//
STDMETHODIMP CDsoDocProperty::QueryInterface(REFIID riid, void** ppv)
{
	HRESULT hr = S_OK;

	ODS("CDsoDocProperty::QueryInterface\n");
	CHECK_NULL_RETURN(ppv, E_POINTER);
	
	if ((IID_CustomProperty == riid) || (IID_IDispatch == riid) || (IID_IUnknown == riid))
    {
		*ppv = (CustomProperty*)this;
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
STDMETHODIMP_(ULONG) CDsoDocProperty::AddRef(void)
{
	TRACE1("CDsoDocProperty::AddRef - %d\n", m_cRef+1);
    if (!m_fExternal) { DllAddRef(); m_fExternal = TRUE; }
    return ++m_cRef;
}

////////////////////////////////////////////////////////////////////////
// Release
//
STDMETHODIMP_(ULONG) CDsoDocProperty::Release(void)
{
	ULONG ul = --m_cRef;
	TRACE1("CDsoDocProperty::Release - %d\n", ul);

    if (ul == 0)
	{
        ASSERT(m_fDeadObj); // We better be dead before going away!
		ODS("CDsoDocProperty Final Release\n");
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
STDMETHODIMP CDsoDocProperty::GetTypeInfoCount(UINT* pctinfo)
{
	ODS("CDsoDocProperty::GetTypeInfoCount\n");
	if (pctinfo) *pctinfo = 1;
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetTypeInfo
//
STDMETHODIMP CDsoDocProperty::GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
{
	ODS("CDsoDocProperty::GetTypeInfo\n");
	CHECK_NULL_RETURN(ppTInfo, E_POINTER);
    *ppTInfo = NULL;

 // We only support default type info...
	CHECK_NULL_RETURN((iTInfo == 0), DISP_E_BADINDEX);

 // Load type info if we have not done so already
    if (NULL == m_ptiDispType)
    {
        HRESULT hr = DsoGetTypeInfo(IID_CustomProperty, &m_ptiDispType);
        RETURN_ON_FAILURE(hr);
    }

    *ppTInfo = m_ptiDispType;
    ADDREF_INTERFACE(((IUnknown*)*ppTInfo));
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// GetIDsOfNames
//
STDMETHODIMP CDsoDocProperty::GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoDocProperty::GetIDsOfNames\n");
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
STDMETHODIMP CDsoDocProperty::Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr)
{
    HRESULT hr;
    ITypeInfo *pti;

	ODS("CDsoDocProperty::Invoke\n");
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
// CustomProperty Implementation
//
////////////////////////////////////////////////////////////////////////
// get_Name
//
STDMETHODIMP CDsoDocProperty::get_Name(BSTR *pbstrName)
{
    ODS("CDsoDocProperty::get_Name\n");
    if (pbstrName) *pbstrName = (m_bstrName ? SysAllocString(m_bstrName) : NULL);
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_Type
//
STDMETHODIMP CDsoDocProperty::get_Type(dsoFilePropertyType *dsoType)
{
	dsoFilePropertyType lType;
	ODS("CDsoDocProperty::get_Type\n");
    switch (m_vValue.vt & VT_TYPEMASK)
    {
      case VT_BSTR: lType = dsoPropertyTypeString; break;
      case VT_I2:
      case VT_I4:   lType = dsoPropertyTypeLong;   break;
      case VT_R4:   
      case VT_R8:   lType = dsoPropertyTypeDouble; break;
      case VT_BOOL: lType = dsoPropertyTypeBool;   break;
      case VT_DATE: lType = dsoPropertyTypeDate;   break;
      default:      lType = dsoPropertyTypeUnknown;
    }
	if (dsoType) *dsoType = lType;
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// get_Value
//
STDMETHODIMP CDsoDocProperty::get_Value(VARIANT *pvValue)
{
    ODS("CDsoDocProperty::get_Value\n");
    CHECK_NULL_RETURN(pvValue, E_POINTER);
    return VariantCopy(pvValue, &m_vValue);
}


////////////////////////////////////////////////////////////////////////
// put_Value
//
STDMETHODIMP CDsoDocProperty::put_Value(VARIANT *pvValue)
{
    VARIANT vtTmp; vtTmp.vt = VT_EMPTY;

    ODS("CDsoDocProperty::put_Value\n");
    CHECK_NULL_RETURN(pvValue, E_POINTER);
    CHECK_FLAG_RETURN((m_fDeadObj || m_fRemovedItem), DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));

 // We don't support arrays (in this sample at least)...
    if ((pvValue->vt) & VT_ARRAY)
        return E_INVALIDARG;

 // Sanity check of VARTYPE (if it is not one we can save, don't bother)...
    switch (((pvValue->vt) & VT_TYPEMASK))
    {
    case VT_I2:
    case VT_I4:
    case VT_R4:
    case VT_R8:
    case VT_DATE:
    case VT_BSTR:
    case VT_BOOL:
        break;
    default:
        return E_INVALIDARG;
    }

 // Swap out the variant value and set the dirty flag. We make independent
 // copy of the VARIANT (performs indirection as needed)...
    m_fModified = TRUE;
    VariantClear(&m_vValue);
    return VariantCopyInd(&m_vValue, pvValue);
}

////////////////////////////////////////////////////////////////////////
// Remove
//
STDMETHODIMP CDsoDocProperty::Remove()
{
	ODS("CDsoDocProperty::Remove\n");
    CHECK_FLAG_RETURN((m_fDeadObj || m_fRemovedItem), DsoReportError(DSO_E_INVALIDOBJECT, NULL, m_pDispExcep));
	VariantClear(&m_vValue);
	m_fRemovedItem = TRUE;
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// Internal Functions
//
////////////////////////////////////////////////////////////////////////
// InitProperty
//
STDMETHODIMP CDsoDocProperty::InitProperty(BSTR bstrName, PROPID propid, VARIANT* pvData, BOOL fNewItem, CDsoDocProperty* pPreviousItem)
{
	TRACE3("CDsoDocProperty::InitProperty(name=%S, id=%d, vtype=%d\n",
		((bstrName) ? bstrName : L"[None]"), propid, (DWORD)((pvData) ? pvData->vt : 0));

    ASSERT(m_bstrName == NULL); ASSERT(m_ulPropID == 0);
    m_bstrName = ((bstrName) ? SysAllocString(bstrName) : NULL);
    m_ulPropID = propid;

    if (FAILED(VariantCopy(&m_vValue, pvData)))
        return E_FAIL;

    m_fNewItem = fNewItem;
    m_pNextItem = pPreviousItem;
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
// AppendLink
//
STDMETHODIMP_(CDsoDocProperty*) CDsoDocProperty::AppendLink(CDsoDocProperty* pLinkItem)
{
    CDsoDocProperty* prev = m_pNextItem;
    m_pNextItem = pLinkItem;
    return prev;
}

////////////////////////////////////////////////////////////////////////
// CreateObject - Creates Property with pre-filled information
//
CDsoDocProperty* CDsoDocProperty::CreateObject(BSTR bstrName, PROPID propid, VARIANT* pvData, BOOL fNewItem, CDsoDocProperty* pPreviousItem)
{
	CDsoDocProperty* pitem = new CDsoDocProperty();
	if (pitem)
	{
		if (FAILED(pitem->InitProperty(bstrName, propid, pvData, fNewItem, pPreviousItem)))
		{
			pitem->Release(); pitem = NULL;
		}
	}
	return pitem;
}
