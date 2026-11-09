import React from 'react';
import { TouchableOpacity, Text, StyleSheet } from 'react-native';
import { Colors } from '../design-tokens/colors';

interface BotonBancarioProps {
  titulo: string;
  onPress: () => void;
  tipo?: 'primary' | 'secondary' | 'danger';
}

export function BotonBancario({ titulo, onPress, tipo = 'primary' }: BotonBancarioProps) {
  const backgroundColor = tipo === 'danger' ? Colors.danger : (tipo === 'secondary' ? Colors.secondary[500] : Colors.primary[500]);

  return (
    <TouchableOpacity style={[styles.boton, { backgroundColor }]} onPress={onPress}>
      <Text style={styles.texto}>{titulo}</Text>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  boton: {
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
    alignItems: 'center',
  },
  texto: {
    color: '#ffffff',
    fontWeight: '600',
    fontSize: 15,
  },
});
