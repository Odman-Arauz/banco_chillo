import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

export function AperturaCajaScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Ventanilla de Cajero — Apertura de Jornada</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center' },
  title: { fontSize: 22, fontWeight: 'bold' }
});
