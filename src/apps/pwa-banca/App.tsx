import React from 'react';
import { StyleSheet, Text, View, SafeAreaView } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { Colors } from './src/shared/design-tokens/colors';

export default function App() {
  return (
    <SafeAreaView style={styles.container}>
      <StatusBar style="auto" />
      <View style={styles.card}>
        <Text style={styles.title}>Banco Chillo</Text>
        <Text style={styles.subtitle}>Banca Web PWA & Plataforma de Servicios</Text>
        <Text style={styles.info}>Módulos: Ventanilla Cajero · Banca Personal · Administración</Text>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
    alignItems: 'center',
    justifyContent: 'center',
  },
  card: {
    backgroundColor: Colors.surface,
    padding: 32,
    borderRadius: 16,
    shadowColor: '#000',
    shadowOpacity: 0.08,
    shadowRadius: 12,
    elevation: 4,
    alignItems: 'center',
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: Colors.primary[500],
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: Colors.text.secondary,
    marginBottom: 16,
  },
  info: {
    fontSize: 13,
    color: Colors.text.muted,
  },
});
