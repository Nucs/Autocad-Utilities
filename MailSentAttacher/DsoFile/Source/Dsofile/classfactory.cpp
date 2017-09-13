/***************************************************************************
 * CLASSFACTORY.CPP
 *
 * CDsoDocumentClassFactory: The Class Factroy for the control.
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
// CDsoDocumentClassFactory - IClassFactory Implementation
//
//  This is a fairly simple CF. We don't provide support for licensing
//  in this sample, nor aggregation. We just create and return a new 
//  CDsoFileDocumentProperties object.
//

////////////////////////////////////////////////////////////////////////
// QueryInterface
//
STDMETHODIMP CDsoDocumentClassFactory::QueryInterface(REFIID riid, void** ppv)
{
	ODS("CDsoDocumentClassFactory::QueryInterface\n");
	CHECK_NULL_RETURN(ppv, E_POINTER);
	
	if (IID_IUnknown == riid || IID_IClassFactory == riid)
	{
		*ppv = (IClassFactory*)this;
		this->AddRef();
		return S_OK;
	}

	*ppv = NULL;
	return E_NOINTERFACE;
}

////////////////////////////////////////////////////////////////////////
// AddRef
//
STDMETHODIMP_(ULONG) CDsoDocumentClassFactory::AddRef(void)
{
	TRACE1("CDsoDocumentClassFactory::AddRef - %d\n", m_cRef+1);
    return ++m_cRef;
}

////////////////////////////////////////////////////////////////////////
// Release
//
STDMETHODIMP_(ULONG) CDsoDocumentClassFactory::Release(void)
{
	TRACE1("CDsoDocumentClassFactory::Release - %d\n", m_cRef-1);
    if (0 != --m_cRef) return m_cRef;

	ODS("CDsoDocumentClassFactory delete\n");
    LockServer(FALSE);
    delete this;
    return 0;
}

////////////////////////////////////////////////////////////////////////
// IClassFactory
//
////////////////////////////////////////////////////////////////////////
// CreateInstance
//
STDMETHODIMP CDsoDocumentClassFactory::CreateInstance(LPUNKNOWN punk, REFIID riid, void** ppv)
{
	HRESULT hr;
	CDsoDocumentProperties* pdfdp;

	ODS("CDsoFileClassFactory::CreateInstance\n");
	CHECK_NULL_RETURN(ppv, E_POINTER);	*ppv = NULL;

 // This version does not support Aggregation...
	if (punk) return CLASS_E_NOAGGREGATION;

 // Create a new instance of the control...
	pdfdp = new CDsoDocumentProperties();
	CHECK_NULL_RETURN(pdfdp, E_OUTOFMEMORY);

 // Initialize the object and QI for requested interface...
	if (SUCCEEDED(hr = pdfdp->InitializeNewInstance()) &&
		SUCCEEDED(hr = pdfdp->QueryInterface(riid, ppv)))
	{
		LockServer(TRUE); // on success, bump up the lock count...
	}
	else {delete pdfdp; *ppv = NULL;} // else cleanup the object

	return hr;
}

////////////////////////////////////////////////////////////////////////
// LockServer
//
STDMETHODIMP CDsoDocumentClassFactory::LockServer(BOOL fLock)
{
	TRACE1("CDsoDocumentClassFactory::LockServer - %d\n", fLock);
	if (fLock) DllAddRef();	else DllRelease();
	return S_OK;
}

