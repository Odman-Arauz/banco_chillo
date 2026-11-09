import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

export function LoginScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Acceso Seguro — Banco Chillo</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center' },
  title: { fontSize: 22, fontWeight: 'bold' }
});
