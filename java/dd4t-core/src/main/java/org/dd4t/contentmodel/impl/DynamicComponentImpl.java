package org.dd4t.contentmodel.impl;

import org.dd4t.contentmodel.DynamicComponent;

import com.tridion.dcp.ComponentPresentation;

public class DynamicComponentImpl extends GenericComponentImpl implements DynamicComponent
{
	private static final long serialVersionUID = 9021378235490500412L;
	private ComponentPresentation nativeDCP;

	public void setNativeDCP(ComponentPresentation nativeDCP){
		this.nativeDCP = nativeDCP;
	}
	
	@Override
	public ComponentPresentation getNativeDCP() {
		return nativeDCP;
	}

}
