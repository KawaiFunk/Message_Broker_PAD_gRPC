package grpc.subscriber;

import java.util.Optional;

public record Config(String host, int port, String topic, boolean tls) {
    public static Config load() {
        String host = getenvOrProp("BROKER_HOST", "broker.host", "localhost");
        int port = Integer.parseInt(getenvOrProp("BROKER_PORT", "broker.port", "50051"));
        String topic = getenvOrProp("BROKER_TOPIC", "broker.topic", "demo.topic");
        boolean tls = Boolean.parseBoolean(getenvOrProp("BROKER_TLS", "broker.tls", "false"));
        return new Config(host, port, topic, tls);
    }
    private static String getenvOrProp(String env, String prop, String def) {
        return Optional.ofNullable(System.getenv(env))
                .orElseGet(() -> System.getProperty(prop, def));
    }
}
