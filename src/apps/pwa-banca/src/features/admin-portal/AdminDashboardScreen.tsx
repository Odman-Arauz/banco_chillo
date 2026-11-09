import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

export function AdminDashboardScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Panel Administrativo de Agencia</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center' },
  title: { fontSize: 22, fontWeight: 'bold' }
});
