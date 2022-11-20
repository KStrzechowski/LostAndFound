import React, { PropsWithChildren } from "react"
import { Pressable, SafeAreaView, StyleSheet, Text, TextInput, TextInputProps, TextProps, View } from "react-native"
import { Colors } from "react-native/Libraries/NewAppScreen"

export const MainContainer: React.FC<PropsWithChildren> = ({ children }) => {
    return (
      <SafeAreaView style={styles.pageContainer}>
        {children}
      </SafeAreaView>
    )
  }
  
export const MainTitle: React.FC<TextProps> = ({ children }) => {
    return (
      <Text style={styles.mainTitle}>
        {children}
      </Text>
    )
  }
  
export const Subtitle: React.FC<TextProps> = ({ children }) => {
    return (
      <Text style={styles.subtitle}>
        {children}
      </Text>
    )
  }
  
export const InputSection: React.FC<
    PropsWithChildren<{
      title: string;
    }>
  > = ({ children, title }) => {
    return (
      <View style={styles.inputSectionContainer}>
        <Text style={styles.inputSectionTitle}>
          {title}
        </Text>
        {children}
      </View>
    );
  };
  
export const CustomTextInput: React.FC<TextInputProps> = ({ onChangeText, keyboardType, placeholder, secureTextEntry }) => {
    return (
      <View style={styles.inputContainer}>
        <TextInput 
          onChangeText={onChangeText} 
          keyboardType={keyboardType} 
          placeholder={placeholder} 
          secureTextEntry={secureTextEntry} />
      </View>
    )
  }
  
export const MainButton: React.FC<
    PropsWithChildren<{
      label: string;
      onPress: any;
    }>
  > = ({ label, onPress }) => {
    return (
      <Pressable style={styles.mainButton} onPress={onPress}>
        <Text style={styles.mainButtonText}>{label}</Text>
      </Pressable>
    );
  };
  
export const PressableText: React.FC<
  PropsWithChildren<{
    text: string;
    onPress: any;
  }>
  > = ({ text, onPress }) => {
  return (
    <Pressable onPress={onPress}>
      <Text style={styles.pressableText}>{text}</Text>
    </Pressable>
  );
  };


  
const styles = StyleSheet.create({
    pageContainer: {
      padding: 30,
      paddingTop: 30,
    },
    mainTitle: {
      fontSize: 24,
      fontWeight: '600',
      color: 'black'
    },
    subtitle: {
      fontSize: 14,
      fontWeight: '400',
      color: 'light-grey'
    },
    inputSectionContainer: {
      paddingTop: 20,
    },
    inputSectionTitle: {
      fontSize: 18,
      fontWeight: '600',
      color: 'black'
    },
    inputContainer: {
      borderBottomWidth: 1,
      borderBottomColor: 'light-grey',
    },
    highlight: {
      fontWeight: '700',
    },
    mainButton: {
      alignItems: 'center',
      margin: 10,
      padding: 20,
      backgroundColor: 'orange',
      borderRadius: 5
    },
    mainButtonText: {
      fontSize: 20,
      fontWeight: "600",
      color: Colors.white
    },
    pressableText: {
      padding: 0,
      margin: 0,
      color: 'orange'
    }
  });