package grpc.subscriber;

import grpc.agent.SubscriberGrpc;
import grpc.agent.SubscribeReply;
import grpc.agent.SubscribeRequest;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.StatusRuntimeException;

import java.util.UUID;
import java.util.concurrent.TimeUnit;

public class ReconnectSubscriberApp {

    public static void main(String[] args) throws InterruptedException {
        // Load configuration
        Config cfg = Config.load();

        // Generate unique client ID
        String clientId = "java-subscriber-" + UUID.randomUUID();

        // Exponential backoff parameters
        long backoffMs = 500;
        final long maxBackoffMs = 10_000;

        while (true) {
            ManagedChannel channel = null;
            try {
                // Print connection details
                System.out.printf("[CONNECTING] Attempting connection to %s:%d, TLS=%b%n",
                        cfg.host(), cfg.port(), cfg.tls());

                // Configure ManagedChannel
                ManagedChannelBuilder<?> builder = ManagedChannelBuilder.forAddress(cfg.host(), cfg.port())
                        .keepAliveTime(30, TimeUnit.SECONDS)
                        .keepAliveTimeout(10, TimeUnit.SECONDS)
                        .keepAliveWithoutCalls(true);

                if (cfg.tls()) {
                    builder.useTransportSecurity();
                } else {
                    builder.usePlaintext();
                }

                channel = builder.build();

                // Create stub
                SubscriberGrpc.SubscriberBlockingStub blockingStub = SubscriberGrpc.newBlockingStub(channel);

                // Build SubscribeRequest
                SubscribeRequest request = SubscribeRequest.newBuilder()
                        .setTopic(cfg.topic())
                        .setAddress("java://" + clientId)
                        .build();

                // Subscribe
                System.out.printf("[SUBSCRIBE] Sending subscription request to topic=%s id=%s%n",
                        cfg.topic(), clientId);

                SubscribeReply reply = blockingStub.subscribe(request);
                System.out.println("[SUBSCRIBE] Subscription successful: success=" + reply.getSuccess());

                // Keep process alive to simulate long-running subscription
                Thread.sleep(60_000);

            } catch (StatusRuntimeException e) {
                System.err.println("[RPC ERROR] " + e.getStatus() + " - " + e.getMessage());
            } catch (Exception e) {
                System.err.println("[FATAL] " + e.getMessage());
            } finally {
                // Shutdown connection
                if (channel != null) {
                    channel.shutdownNow();
                    channel.awaitTermination(5, TimeUnit.SECONDS);
                }
            }
            // Exponential backoff for retries
            System.out.printf("[RETRY] Reconnecting in %d ms...%n", backoffMs);
            Thread.sleep(backoffMs);
            backoffMs = Math.min(backoffMs * 2, maxBackoffMs);
        }
    }
}