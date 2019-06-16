# Button2Key

Simple mapping utility to enable mapping of a DirectInput compatible controller to simulated keyboard input.

## Introduction

Button2Key is a simple yet powerful command line tool for mapping DirectInput buttons to sequences of keys.

## Getting Started

The quickest way to get started would be to run the application and then type the command,

    > run example.txt
    
This should cause Button2Key to read the text document included with the project named "example.txt".  Assuming that it is located in the same directory as the application executable, the relative path should work fine.

The contents of example.txt is as follows,

    //This is an example of a command file
    //when loaded by Button2Key, each line
    //will be processed in sequence.  The
    //commands specified in this file will
    //map the following for Assetto Corsa
    //and a Logitech G29 Steering Wheel

    //Reset VR Camera -> L2
    //Reset Session -> R2

    //Run this configuration by entering the command
    //(assuming that this file is called example.txt,
    //and is in the same path as this Button2Key.exe)
    //
    //> run example.txt

    add type=keyboardinput name=assettocorsa_resetvrcam description="Reset VR Camera" command="keydown:control;keydown:space;sleep:100;keyup:space;keyup:control"
    add type=keyboardinput name=assettocorsa_resetsession description="Reset Session" command="keydown:control;keydown:vk_o;sleep:100;keyup:vk_o;keyup:control"
    add type=mapping button=Buttons7 value="equals:128" input=assettocorsa_resetvrcam
    add type=mapping button=Buttons6 value="equals:128" input=assettocorsa_resetsession
    listen productguid=c24f046d-0000-0000-0000-504944564944 debug=off
    
 As the comments of the file suggest, this will map 2 buttons on the Logitech G29 steering wheel to 2 Asseto Corsa keyboard shortcuts.  L2 will become the "Reset VR Camera" option. And R2 will become "Restart Session".
 
 More information to follow...
 
## Commands (* denotes default parameter, label is not required)


  **run** input*="&lt;full or relative path&gt;"
  
  *Run a series of commands from a file.*
  
  **Example:**
  
  Run the commands in the file example.txt located in this application path
  
    run example.txt
  
  Run the commands in the file example.txt located in "c:\temp\"
  
    run input="c:\temp\example.txt"
  
  ---
  
  **list** type*=&lt;type of objects to list&gt;
  
  *Lists objects currently loaded by Button2Key
  
  **Types:**
  
  devices  - *DirectInput devices*
  
  inputs - *All configured inputs*
  
  mappings - *All configured mappings*
  
  **Example:**
  
    list devices  //List all DirectInput devices
  
  ---
  
  **listen** [productguid=<product guid of input device>] [instanceguid=<instance guid of input device>] [debug=&lt;on\off&gt;]
  
  *Start listening to the DirectInput device matching the provided guid*
  *You should only provide either productguid or instanceguid, not both.  These values can be obtained through the list command.*
  
  **Example:**
  
  Listen for activity on a Logitech G29 Steering Wheel. Output all controller activity.
  
    listen productguid=c24f046d-0000-0000-0000-504944564944 debug=on
  
  ---
  
  **add** type*=&lt;type of object to add&gt; 
  
  **Types:**
  
  keyboardinput  - *Keyboard Input, specify a sequence of keys to simulate*
  
  Additional Parameters Required:
  
  [name=&lt;Name to give keyboard input&gt;]
  [description=&lt;Description to give keyboard input&gt;]
  [command=&lt;Sequence of keys to simulate&gt;]
  
  ---

  mapping - *Mapping, specify what input to process when a specific button value is detected*
  
  [value=&lt;Value of button to match&gt;]
  [input=&lt;Name of the input to process upon matching&gt;]

  Additional Parameters Required:

  **Example:**
  
  Add a keyboard input named 'assettocorsa_resetvrcam' that performs a CTRL+SPACE combination for 100 milliseconds before releasing the keys.
  
    add type=keyboardinput name=assettocorsa_resetvrcam description="Reset VR Camera" command="keydown:control;keydown:space;sleep:100;keyup:space;keyup:control"

  Add a mapping so that when 'Button7' of the device being listened to equals 128, the keyboard input named 'assettocorsa_resetvrcam' will be performed.

    add type=mapping button=Buttons7 value="equals:128" input=assettocorsa_resetvrcam
  
  ---