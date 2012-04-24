
package com.acme.api;

import java.util.List;

public class Person{
   	private String gender;
   	private Name name;
   	private String userImage;
   	private boolean verified;

 	public String getGender(){
		return this.gender;
	}
	public void setGender(String gender){
		this.gender = gender;
	}
 	public Name getName(){
		return this.name;
	}
	public void setName(Name name){
		this.name = name;
	}
 	public String getUserImage(){
		return this.userImage;
	}
	public void setUserImage(String userImage){
		this.userImage = userImage;
	}
 	public boolean getVerified(){
		return this.verified;
	}
	public void setVerified(boolean verified){
		this.verified = verified;
	}
}
