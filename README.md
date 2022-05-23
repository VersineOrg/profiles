# The Profiles microservice is archived and isn't used in versine anymore.
#
#
#############################################################
#                         Profiles                          #
#############################################################

Autor: Mickael

Last edited: the 26th of april 2022

The role:

This is the profiles Mirco service,
It's goal is to recieve request asking information about a user, 
check if the asker is authaurized to acces this profile through his token and
return all the public information about him in a json format.

The reqest:

This is an example of a request to this micro service:

http://hostname:port/profile/username

Method:POST

BODY:
{
"token":"ewogICJhbGdvIjogIkhTMjU2IiwKICAidHlwZSI6ICJKV1QiCn0=.ewogICJpZCI6ICI2MjY4MTc5M2Q0NjMyNGMzOTE5YjIzMTEiLAogICJleHAiOiAiMCIKfQ==.Pz4kags/CHIFPyxLPxF2Pz8pSj8vP14/VVU/P3U0JT8="

}

Here the user gives his Token to authenticate himself as legitimate asker for thoses informations.

The response:

This is an example of response:

"{\"user\":{\"name\":\"pkngr\",\"Avatar\":\"https://google.com/img/48294.png\",\"bio\":\"j'aime les frites\",\"banner\":\"https://images7.alphacoders.com/421/thumb-1920-421957.jpg\",\"color\":\"28DBB7\",\"id\":\"62681793d46324c3919b2311\"}}"
}

Features to implement in the future:

change the amount of informations provided depending on the relation betwwen the Target and the Asker.


