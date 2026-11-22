import React, { useEffect, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { Colors } from './src/shared/design-tokens/colors';
import { EstadoGateway, GATEWAY_URL, verificarGateway } from './src/core/salud';

// Pantalla base. El Issue de la PWA (#7) reemplaza esto por la navegación por rol:
// Login → (Socio: banca-personal) | (Cajero: cajero-ventanilla) | (Admin: admin-portal).

const TEXTO_ESTADO: Record<EstadoGateway, string> = {
  verificando: 'Verificando conexión…',
  'en-linea': 'API Gateway en línea',
  'sin-conexion': 'Sin conexión con el API Gateway',
};

export default function App() {
  const [estado, setEstado] = useState<EstadoGateway>('verificando');

  useEffect(() => {
    verificarGateway().then(setEstado);
  }, []);

  return (
    <View style={styles.container}>
      <StatusBar style="auto" />
      <View style={styles.card}>
        <Text accessibilityRole="header" style={styles.title}>
          Banco Chillo
        </Text>
        <Text style={styles.subtitle}>Banca Web PWA y Plataforma de Servicios</Text>
        <Text style={styles.info}>Módulos: Ventanilla Cajero · Banca Personal · Administración</Text>
        <Text
          testID="estado-gateway"
          style={[styles.estado, { color: estado === 'en-linea' ? Colors.success : Colors.danger }]}>
          {TEXTO_ESTADO[estado]}
        </Text>
        <Text style={styles.info}>{GATEWAY_URL}</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 16,
  },
  card: {
    backgroundColor: Colors.surface,
    padding: 32,
    borderRadius: 16,
    boxShadow: '0 4px 12px rgba(0, 0, 0, 0.08)',
    alignItems: 'center',
    gap: 8,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: Colors.primary[500],
  },
  subtitle: {
    fontSize: 16,
    color: Colors.text.secondary,
  },
  info: {
    fontSize: 13,
    color: Colors.text.secondary,
  },
  estado: {
    marginTop: 8,
    fontSize: 15,
    fontWeight: '600',
  },
});
